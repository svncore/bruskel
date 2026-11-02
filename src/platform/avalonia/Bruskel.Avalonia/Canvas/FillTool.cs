using System;
using Bruskel.Core;

namespace Bruskel.Avalonia.Canvas;

internal sealed class FillTool : ICanvasTool
{
	public CanvasHost.CanvasTool Kind => CanvasHost.CanvasTool.Fill;
	public bool PointerPressed (CanvasToolPointerContext context)
		=> context.Host.FillAt (context.Point, context.Color, context.ShiftPressed);

	public bool Fill (IImageSurface? surface, PointD point, ColorBgra color, int tolerance, bool global,
		Action beginStroke, Action commitStroke, bool[]? allowed = null)
	{
		if (!RasterAlgorithms.TrySampleNearest (surface, point, out ColorBgra target) || target == color)
			return false;
		int x = (int) Math.Round (point.X), y = (int) Math.Round (point.Y);
		if (allowed is not null && !allowed[y * surface!.Width + x]) return false;
		bool[]? mask = global ? FloodStencil.ByColor (surface!, x, y, tolerance, allowed)
			: FloodStencil.FromPoint (surface!, x, y, tolerance, allowed);
		if (mask is null) return false;
		beginStroke ();
		Span<ColorBgra> pixels = surface!.GetPixelData ();
		for (int i = 0; i < pixels.Length; i++)
			if (mask[i]) pixels[i] = color;
		commitStroke ();
		return true;
	}
}
