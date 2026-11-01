using Avalonia;
using System;
using Pinta.Core;
using Pintora.Avalonia.Document;

namespace Pintora.Avalonia.Canvas;

internal sealed class ZoomTool : ICanvasTool
{
	public CanvasHost.CanvasTool Kind => CanvasHost.CanvasTool.Zoom;
	internal readonly record struct ZoomResult (double Scale, Vector Offset);
	private Point? start_view;
	private PointD start_canvas;
	private bool right_button;
	public bool IsDragging => start_view is not null;
	public Point? DragStart => start_view;
	public PointD DragStartCanvas => start_canvas;

	public bool PointerPressed (CanvasToolPointerContext context)
	{
		start_view = context.ViewPoint;
		start_canvas = context.Point;
		right_button = context.IsRightButton;
		return true;
	}

	public bool PointerMoved (CanvasToolPointerContext context)
	{
		if (start_view is not Point origin)
			return false;
		if (!right_button && Distance (context.ViewPoint, origin) > 10)
			context.Host.InvalidateToolVisual ();
		return true;
	}

	public bool PointerReleased (CanvasToolPointerContext context)
	{
		if (start_view is not Point origin)
			return false;
		if (right_button)
			context.Host.ZoomAt (context.ViewPoint, 1 / 1.25);
		else if (Distance (context.ViewPoint, origin) > 10)
			context.Host.ZoomToCanvasRectangle (start_canvas, context.Point);
		else
			context.Host.ZoomAt (origin, 1.25);
		start_view = null;
		return true;
	}

	public void CancelPointerAction () => start_view = null;

	private static double Distance (Point a, Point b)
	{
		double dx = a.X - b.X, dy = a.Y - b.Y;
		return Math.Sqrt (dx * dx + dy * dy);
	}

	public ZoomResult Calculate (Point viewPoint, double factor, double currentScale, Point canvasOrigin,
		Point centeredCanvasOrigin, DocumentViewportState viewport)
	{
		PointD canvasPoint = viewport.ViewPointToCanvas (new PointD (viewPoint.X - canvasOrigin.X, viewPoint.Y - canvasOrigin.Y));
		double nextScale = System.Math.Clamp (currentScale * factor, 0.1, 32);
		var nextOffset = new Vector (
			viewPoint.X - centeredCanvasOrigin.X - canvasPoint.X * nextScale,
			viewPoint.Y - centeredCanvasOrigin.Y - canvasPoint.Y * nextScale);
		return new ZoomResult (nextScale, nextOffset);
	}
}
