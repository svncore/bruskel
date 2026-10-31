using System;
using Pinta.Core;

namespace Pintora.Avalonia.Canvas;

/// <summary>
/// Paint.NET-style move tools. Dragging moves the selection outline live;
/// Move Selected Pixels additionally commits the pixel move once on release,
/// so a drag produces exactly one history entry. Rounding is absolute from
/// the drag origin — no cumulative drift across pointer-move events.
/// </summary>
internal sealed class MoveTool : ICanvasTool
{
	public CanvasHost.CanvasTool Kind { get; }

	private PointD start;
	private bool active;
	private PointI applied;

	public MoveTool (CanvasHost.CanvasTool kind) => Kind = kind;

	private bool MovesPixels => Kind == CanvasHost.CanvasTool.MoveSelectedPixels;

	public bool PointerPressed (CanvasToolPointerContext context)
	{
		CanvasHost host = context.Host;
		if (host.Selection is null || (MovesPixels && host.IsActiveLayerLocked))
			return false;
		start = context.Point;
		applied = new PointI (0, 0);
		active = true;
		return true;
	}

	public bool PointerMoved (CanvasToolPointerContext context)
	{
		if (!active)
			return false;
		var total = new PointI ((int) Math.Round (context.Point.X - start.X), (int) Math.Round (context.Point.Y - start.Y));
		var step = new PointI (total.X - applied.X, total.Y - applied.Y);
		if (step.X != 0 || step.Y != 0) {
			context.Host.TranslateSelection (step);
			applied = total;
		}
		return true;
	}

	public bool PointerReleased (CanvasToolPointerContext context)
	{
		if (!active)
			return false;
		active = false;
		if (MovesPixels && applied is { X: not 0 } or { Y: not 0 }) {
			// The outline already moved live; put it back and commit the pixel
			// move from the original position in one stroke.
			context.Host.TranslateSelection (new PointI (-applied.X, -applied.Y));
			context.Host.MoveSelection (applied);
		}
		return true;
	}

	public void CancelPointerAction ()
	{
		if (!active)
			return;
		active = false;
		// A cancel with a live outline move restores the outline only; pixels
		// are untouched because the commit happens on release.
	}
}
