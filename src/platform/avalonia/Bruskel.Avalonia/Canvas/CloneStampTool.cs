using System;
using Bruskel.Core;

namespace Bruskel.Avalonia.Canvas;

/// <summary>
/// Paint.NET-style Clone Stamp. Ctrl+click sets the source point; a drag then
/// copies pixels from source + (cursor - drag anchor), keeping the offset fixed
/// for the whole stroke from a per-stroke snapshot of the layer taken at
/// pointer-press, so an overlapping source/destination never re-reads pixels the
/// stroke has already written. Both mouse buttons paint. The source persists
/// across strokes until re-set with another Ctrl+click.
/// </summary>
internal sealed class CloneStampTool : ICanvasTool
{
	public CanvasHost.CanvasTool Kind => CanvasHost.CanvasTool.CloneStamp;

	private PointD? source;
	private bool pointer_active;
	private PointD anchor;
	private PointD stroke_source;
	private ColorBgra[]? stroke_snapshot;
	private bool[]? selection_mask;

	public bool HasSource => source is not null;

	public void ClearSource () => source = null;

	public bool PointerPressed (CanvasToolPointerContext context)
	{
		CanvasHost host = context.Host;
		if (context.ControlPressed) {
			source = context.Point;
			return true;
		}
		if (source is null || host.IsActiveLayerLocked || host.Surface is null)
			return false;
		host.BeginStroke ();
		selection_mask = host.CaptureSelectionMask ();
		pointer_active = true;
		anchor = context.Point;
		stroke_source = source.Value;
		stroke_snapshot = host.Surface.GetPixelData ().ToArray ();
		bool changed = CopyStamp (host.Surface, context.Point, stroke_source,
			host.BrushSize, host.BrushOpacity, host.IsActiveLayerLocked, stroke_snapshot);
		if (changed)
			host.MarkCanvasChanged (true);
		return true;
	}

	public bool PointerMoved (CanvasToolPointerContext context)
	{
		if (!pointer_active)
			return false;
		CanvasHost host = context.Host;
		bool changed = false;
		foreach (PointD point in BrushAlgorithms.InterpolateLine (context.PreviousPoint, context.Point))
			changed |= CopyStamp (host.Surface, point, new PointD (
				stroke_source.X + point.X - anchor.X,
				stroke_source.Y + point.Y - anchor.Y),
				host.BrushSize, host.BrushOpacity, host.IsActiveLayerLocked, stroke_snapshot);
		if (changed)
			host.MarkCanvasChanged (true);
		return true;
	}

	public bool PointerReleased (CanvasToolPointerContext context)
	{
		bool was_active = pointer_active;
		pointer_active = false;
		stroke_snapshot = null;
		return was_active;
	}

	public void CancelPointerAction ()
	{
		pointer_active = false;
		stroke_snapshot = null;
	}

	/// <summary>
	/// Copies a square block of <paramref name="size"/> pixels centered on
	/// <paramref name="sourceOrigin"/> over the block centered on
	/// <paramref name="destination"/>, honoring <paramref name="opacity"/>
	/// through alpha compositing. Returns false when painting is not possible.
	/// When <paramref name="sampleFrom"/> is provided it must be a full-surface
	/// snapshot whose pixels are read instead of the live surface, so a stroke
	/// never re-reads pixels it has already written.
	/// </summary>
	public bool CopyStamp (IImageSurface? surface, PointD destination, PointD sourceOrigin,
		int size, double opacity, bool layerLocked, ReadOnlySpan<ColorBgra> sampleFrom = default)
	{
		if (layerLocked || surface is null)
			return false;
		int radius = Math.Max (size, 1) / 2;
		int dstX = (int) Math.Round (destination.X), dstY = (int) Math.Round (destination.Y);
		int srcX = (int) Math.Round (sourceOrigin.X), srcY = (int) Math.Round (sourceOrigin.Y);
		Span<ColorBgra> pixels = surface.GetPixelData ();
		for (int dy = -radius; dy <= radius; dy++)
			for (int dx = -radius; dx <= radius; dx++) {
				int tx = dstX + dx, ty = dstY + dy;
				int sx = srcX + dx, sy = srcY + dy;
				if ((uint) tx < surface.Width && (uint) ty < surface.Height
					&& (uint) sx < surface.Width && (uint) sy < surface.Height) {
					ColorBgra sampled = sampleFrom.IsEmpty
						? pixels[sy * surface.Width + sx]
						: sampleFrom[sy * surface.Width + sx];
					if (sampled.A > 0 && opacity < 1)
						sampled = ColorBgra.FromBgra (sampled.B, sampled.G, sampled.R,
							(byte) Math.Round (sampled.A * Math.Clamp (opacity, 0, 1)));
					int index = ty * surface.Width + tx;
					if (selection_mask is not null && !selection_mask[index]) continue;
					pixels[index] = RasterAlgorithms.CompositeOver (pixels[index], sampled);
				}
			}
		return true;
	}
}
