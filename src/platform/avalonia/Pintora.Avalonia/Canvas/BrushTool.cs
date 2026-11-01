using System;
using Pinta.Core;

namespace Pintora.Avalonia.Canvas;

internal sealed class BrushTool : ICanvasTool
{
	public BrushTool (CanvasHost.CanvasTool kind) => Kind = kind;
	public CanvasHost.CanvasTool Kind { get; }
	public bool IsPencil => Kind == CanvasHost.CanvasTool.Pencil;
	public bool IsEraser => Kind == CanvasHost.CanvasTool.Eraser;
	private bool pointer_active;
	private PointD previous_point;
	private double spacing_accum;
	private PointD? smooth_point;
	private readonly Random random = new (17);
	private int splatter_min = 5, splatter_max = 10, slash_angle = 45;

	/// <summary>
	/// Input-smoothing step: pulls the raw pointer position towards the
	/// previous smoothed position. Smoothing 0 returns the raw point untouched;
	/// higher values make the stroke trail the pointer more loosely.
	/// </summary>
	public static PointD ApplySmoothing (PointD? previousSmoothed, PointD raw, double smoothing)
	{
		if (smoothing <= 0 || previousSmoothed is not PointD previous)
			return raw;
		double alpha = 1 - Math.Clamp (smoothing, 0, 1) * 0.85;
		return new PointD (
			previous.X + (raw.X - previous.X) * alpha,
			previous.Y + (raw.Y - previous.Y) * alpha);
	}

	public bool PointerPressed (CanvasToolPointerContext context)
	{
		CanvasHost host = context.Host;
		splatter_min = host.BrushSplatterMinSize;
		splatter_max = host.BrushSplatterMaxSize;
		slash_angle = host.BrushSlashAngle;
		double size = EffectiveSize (host, context.Pressure);
		if (!BeginStroke (host.Surface, context.Point, context.Color, size,
			host.BrushOpacity, host.BrushHardness, host.IsActiveLayerLocked, host.BeginStroke, host.BrushTipShape, host.BrushPreset))
			return false;
		pointer_active = true;
		previous_point = context.Point;
		spacing_accum = 0;
		smooth_point = null;
		host.MarkCanvasChanged (true);
		return true;
	}

	public bool PointerMoved (CanvasToolPointerContext context)
	{
		if (!pointer_active)
			return false;
		CanvasHost host = context.Host;
		PointD target = ApplySmoothing (smooth_point, context.Point, host.BrushSmoothing);
		if (host.BrushSmoothing > 0)
			smooth_point = target;
		bool changed = StampSegment (host.Surface, previous_point, target, context.Color,
			EffectiveSize (host, context.Pressure), host.BrushOpacity, host.BrushHardness, host.BrushSpacing, host.IsActiveLayerLocked, host.BrushTipShape, host.BrushPreset);
		previous_point = target;
		if (changed)
			host.MarkCanvasChanged (true);
		return true;
	}

	private static double EffectiveSize (CanvasHost host, double pressure)
		=> host.BrushPressureEnabled ? host.BrushSizePrecise * Math.Clamp (pressure, 0.1, 1) : host.BrushSizePrecise;

	public bool PointerReleased (CanvasToolPointerContext context)
	{
		bool wasActive = pointer_active;
		pointer_active = false;
		smooth_point = null;
		return wasActive;
	}

	public void CancelPointerAction ()
	{
		pointer_active = false;
		smooth_point = null;
	}

	public bool BeginStroke (IImageSurface? surface, PointD point, ColorBgra color, int size,
		double opacity, double hardness, bool layerLocked, Action beginHistory)
		=> BeginStroke (surface, point, color, (double) size, opacity, hardness, layerLocked, beginHistory, BrushTipShape.Square);

	public bool BeginStroke (IImageSurface? surface, PointD point, ColorBgra color, double size,
		double opacity, double hardness, bool layerLocked, Action beginHistory, BrushTipShape tipShape = BrushTipShape.Round, BrushPreset preset = BrushPreset.Normal)
	{
		if (layerLocked || surface is null)
			return false;
		beginHistory ();
		return IsPencil
			? DrawPencilLine (surface, point, point, color, opacity)
			: DrawPoint (surface, point, color, size, opacity, hardness, IsEraser, tipShape, preset);
	}

	public bool ContinueStroke (IImageSurface? surface, PointD from, PointD to, ColorBgra color,
		int size, double opacity, double hardness, bool layerLocked)
	{
		if (layerLocked || surface is null)
			return false;
		return IsPencil
			? DrawPencilLine (surface, from, to, color, opacity)
			: DrawLine (surface, from, to, color, size, opacity, hardness, IsEraser);
	}

	/// <summary>
	/// Stamps brush tips along the segment at <paramref name="spacing"/> fractions
	/// of the brush width (Paint.NET spacing). Interpolated at ~1 px density, so
	/// spacing below one pixel degenerates to continuous stamping.
	/// </summary>
	public bool StampSegment (IImageSurface? surface, PointD from, PointD to, ColorBgra color,
		double size, double opacity, double hardness, double spacing, bool layerLocked, BrushTipShape tipShape = BrushTipShape.Round, BrushPreset preset = BrushPreset.Normal)
	{
		if (layerLocked || surface is null)
			return false;
		if (IsPencil)
			return DrawPencilLine (surface, from, to, color, opacity);
		if (from == to)
			return DrawPoint (surface, to, color, size, opacity, hardness, IsEraser, tipShape, preset);
		double spacingPixels = Math.Max (1, size * Math.Clamp (spacing, 0.02, 4));
		bool changed = false;
		foreach (PointD point in BrushAlgorithms.InterpolateLine (from, to)) {
			spacing_accum += 1;
			if (spacing_accum < spacingPixels)
				continue;
			spacing_accum = 0;
			changed |= DrawPoint (surface, point, color, size, opacity, hardness, IsEraser, tipShape, preset);
		}
		return changed;
	}
	public bool DrawPoint (IImageSurface? surface, PointD point, ColorBgra color, int size,
		double opacity, double hardness, bool erase)
		=> DrawPoint (surface, point, color, (double) size, opacity, hardness, erase, BrushTipShape.Square);

	public bool DrawPoint (IImageSurface? surface, PointD point, ColorBgra color, double size,
		double opacity, double hardness, bool erase, BrushTipShape tipShape = BrushTipShape.Round, BrushPreset preset = BrushPreset.Normal, double tipAngle = 0)
	{
		if (surface is null)
			return false;
		if (!erase && preset != BrushPreset.Normal)
			return DrawPresetStamp (surface, point, color, size, opacity, hardness, tipShape, preset);
		double radius = Math.Max (size - 1, 0) / 2;
		Span<ColorBgra> pixels = surface.GetPixelData ();
		int centerX = (int) Math.Round (point.X), centerY = (int) Math.Round (point.Y);
		if (color.A > 0 && opacity < 1)
			color = ColorBgra.FromBgra (color.B, color.G, color.R, (byte) Math.Round (color.A * Math.Clamp (opacity, 0, 1)));
		int minX = (int) Math.Floor (centerX - radius), maxX = (int) Math.Ceiling (centerX + radius);
		int minY = (int) Math.Floor (centerY - radius), maxY = (int) Math.Ceiling (centerY + radius);
		for (int y = minY; y <= maxY; y++)
			for (int x = minX; x <= maxX; x++)
				if ((uint) x < surface.Width && (uint) y < surface.Height) {
					ColorBgra painted = erase ? ColorBgra.Zero : color;
					double radians = -tipAngle * Math.PI / 180;
					double rotatedX = (x - centerX) * Math.Cos (radians) - (y - centerY) * Math.Sin (radians);
					double rotatedY = (x - centerX) * Math.Sin (radians) + (y - centerY) * Math.Cos (radians);
					if (tipShape == BrushTipShape.Round && radius > 0 &&
						Math.Sqrt ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY)) > radius)
						continue;
					if (tipShape == BrushTipShape.Square && (Math.Abs (rotatedX) > radius || Math.Abs (rotatedY) > radius))
						continue;
					if (color.A > 0 && hardness < 1 && radius > 0) {
						double edge = Math.Sqrt ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY)) / radius;
						double softness = BrushAlgorithms.HardnessCoverage (edge, hardness);
						painted = softness <= 0 ? ColorBgra.Zero : ColorBgra.FromBgra (color.B, color.G, color.R, (byte) Math.Round (color.A * softness));
					}
					int index = y * surface.Width + x;
					pixels[index] = erase ? painted : RasterAlgorithms.CompositeOver (pixels[index], painted);
				}
		return true;
	}

	private bool DrawPresetStamp (IImageSurface surface, PointD point, ColorBgra color, double size,
		double opacity, double hardness, BrushTipShape tipShape, BrushPreset preset)
	{
		int count = preset == BrushPreset.Splatter ? 8 : preset == BrushPreset.Grid ? 5 : 1;
		for (int i = 0; i < count; i++) {
			PointD offset = preset switch {
				BrushPreset.Splatter => new (random.NextDouble () * size - size / 2, random.NextDouble () * size - size / 2),
				BrushPreset.Grid => new ((i % 3 - 1) * size, (i / 3 - 1) * size),
				_ => PointD.Zero
			};
			double stampSize = preset == BrushPreset.Splatter
				? Math.Clamp (splatter_min + random.Next (Math.Max (1, splatter_max - splatter_min + 1)), 1, 10000)
				: size;
			if (preset == BrushPreset.Circles)
				stampSize = Math.Max (1, size * (0.35 + i * 0.2));
			DrawPoint (surface, point + offset, color, stampSize, opacity * (preset == BrushPreset.Splatter ? 0.65 : 1), hardness, false,
				preset == BrushPreset.Squares || preset == BrushPreset.Slash ? BrushTipShape.Square : tipShape, BrushPreset.Normal,
				preset == BrushPreset.Slash ? slash_angle : 0);
		}
		return true;
	}

	public bool DrawLine (IImageSurface? surface, PointD from, PointD to, ColorBgra color,
		int size, double opacity, double hardness, bool erase)
		=> DrawLine (surface, from, to, color, (double) size, opacity, hardness, erase, BrushTipShape.Square);

	public bool DrawLine (IImageSurface? surface, PointD from, PointD to, ColorBgra color,
		double size, double opacity, double hardness, bool erase, BrushTipShape tipShape = BrushTipShape.Round)
	{
		if (surface is null)
			return false;
		if (from == to)
			return DrawPoint (surface, from, color, size, opacity, hardness, erase, tipShape);
		foreach (PointD point in BrushAlgorithms.InterpolateLine (from, to))
			DrawPoint (surface, point, color, size, opacity, hardness, erase, tipShape);
		return true;
	}

	public bool DrawPencilLine (IImageSurface? surface, PointD from, PointD to, ColorBgra color, double opacity)
	{
		if (surface is null)
			return false;
		if (color.A > 0 && opacity < 1)
			color = ColorBgra.FromBgra (color.B, color.G, color.R, (byte) Math.Round (color.A * Math.Clamp (opacity, 0, 1)));
		Span<ColorBgra> pixels = surface.GetPixelData ();
		if (from == to) {
			int x = (int) Math.Round (from.X), y = (int) Math.Round (from.Y);
			if ((uint) x < surface.Width && (uint) y < surface.Height) {
				int index = y * surface.Width + x;
				pixels[index] = RasterAlgorithms.CompositeOver (pixels[index], color);
			}
			return true;
		}
		foreach (PointD point in BrushAlgorithms.InterpolateLine (from, to)) {
			int x = (int) Math.Round (point.X);
			int y = (int) Math.Round (point.Y);
			if ((uint) x < surface.Width && (uint) y < surface.Height) {
				int index = y * surface.Width + x;
				pixels[index] = RasterAlgorithms.CompositeOver (pixels[index], color);
			}
		}
		return true;
	}

}
