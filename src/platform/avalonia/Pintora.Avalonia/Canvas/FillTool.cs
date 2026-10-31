using System;
using Pinta.Core;

namespace Pintora.Avalonia.Canvas;

internal sealed class FillTool : ICanvasTool
{
	public CanvasHost.CanvasTool Kind => CanvasHost.CanvasTool.Fill;
	public bool PointerPressed (CanvasToolPointerContext context)
		=> context.Host.FillAt (context.Point, context.Color, context.ShiftPressed);

	public bool Fill (IImageSurface? surface, PointD point, ColorBgra color, int tolerance, bool global,
		Action beginStroke, Action commitStroke)
	{
		if (!RasterAlgorithms.TrySampleNearest (surface, point, out ColorBgra target) || target == color)
			return false;
		beginStroke ();
		if (global) {
			// Global fill: paint every pixel within tolerance of the sampled
			// color across the whole layer (Paint.NET Shift-mode).
			Span<ColorBgra> pixels = surface!.GetPixelData ();
			for (int i = 0; i < pixels.Length; i++)
				if (ColorBgra.ColorsWithinTolerance (target, pixels[i], tolerance))
					pixels[i] = color;
		} else {
			RasterAlgorithms.FloodFill (surface, point, color, tolerance);
		}
		commitStroke ();
		return true;
	}
}
