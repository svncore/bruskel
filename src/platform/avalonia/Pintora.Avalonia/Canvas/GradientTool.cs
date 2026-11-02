using System;
using Avalonia;
using Avalonia.Media;
using Pinta.Core;
using Pintora.Avalonia.Imaging;

namespace Pintora.Avalonia.Canvas;

/// <summary>
/// Paint.NET-style Gradient tool. Drag paints a live-previewed linear gradient;
/// Shift constrains the drag angle to 15° increments. After a gradient is
/// committed, its endpoints stay editable: dragging a nub re-renders the
/// gradient live from a snapshot (one history entry per adjustment), dragging
/// the gradient line moves the whole gradient, and dragging elsewhere starts a
/// new gradient.
/// </summary>
internal sealed class GradientTool : ICanvasTool
{
	public CanvasHost.CanvasTool Kind => CanvasHost.CanvasTool.Gradient;
	public GradientToolOptions Options { get; } = new ();

	private enum NubDrag { None, StartNub, EndNub, Move }

	private PointD start;
	private PointD end;
	private ColorBgra primary;
	private ColorBgra secondary;
	private bool reversed;
	public bool IsActive { get; private set; }

	public bool HasCommitted { get; private set; }
	public PointD CommittedStart { get; private set; }
	public PointD CommittedEnd { get; private set; }

	private NubDrag nub_drag;
	private PointD move_anchor;
	private PointD move_start;
	private PointD move_end;
	private ColorBgra[]? drag_snapshot;
	private bool[]? selection_mask;

	public bool PointerPressed (CanvasToolPointerContext context)
	{
		CanvasHost host = context.Host;
		if (host.IsActiveLayerLocked || host.Surface is null)
			return false;
		selection_mask = host.CaptureSelectionMask ();
		if (HasCommitted && !IsActive) {
			double nubTolerance = Math.Max (4, 6 / Math.Max (host.Scale, 0.01));
			NubDrag mode;
			if (Distance (context.Point, CommittedStart) <= nubTolerance)
				mode = NubDrag.StartNub;
			else if (Distance (context.Point, CommittedEnd) <= nubTolerance)
				mode = NubDrag.EndNub;
			else if (DistanceToSegment (context.Point, CommittedStart, CommittedEnd) <= nubTolerance)
				mode = NubDrag.Move;
			else
				mode = NubDrag.None;
			if (mode != NubDrag.None) {
				nub_drag = mode;
				move_anchor = context.Point;
				move_start = CommittedStart;
				move_end = CommittedEnd;
				drag_snapshot = host.CaptureActiveLayerPixels ();
				host.BeginStroke ();
				return true;
			}
		}
		// Right-drag draws a secondary-to-primary gradient on the fly, matching
		// the Paint.NET behavior (the Reverse option still applies).
		Begin (context.Point, host.BrushColor, host.SecondaryBrushColor, Options.Reversed ^ context.IsRightButton);
		return true;
	}

	public bool PointerMoved (CanvasToolPointerContext context)
	{
		CanvasHost host = context.Host;
		if (IsActive) {
			Update (context.ShiftPressed ? ConstrainAngle (start, context.Point) : context.Point);
			host.InvalidateToolVisual ();
			return true;
		}
		if (nub_drag == NubDrag.None)
			return false;
		if (host.Surface is null || drag_snapshot is null)
			return false;
		PointD newStart = move_start, newEnd = move_end;
		switch (nub_drag) {
			case NubDrag.StartNub:
				newStart = context.ShiftPressed ? ConstrainAngle (newEnd, context.Point) : context.Point;
				break;
			case NubDrag.EndNub:
				newEnd = context.ShiftPressed ? ConstrainAngle (newStart, context.Point) : context.Point;
				break;
			case NubDrag.Move:
				var delta = new PointD (context.Point.X - move_anchor.X, context.Point.Y - move_anchor.Y);
				newStart = new PointD (move_start.X + delta.X, move_start.Y + delta.Y);
				newEnd = new PointD (move_end.X + delta.X, move_end.Y + delta.Y);
				break;
		}
		host.RestoreActiveLayerPixels (drag_snapshot);
		ApplyClipped (host.Surface, newStart, newEnd);
		CommittedStart = newStart;
		CommittedEnd = newEnd;
		host.MarkCanvasChanged (true);
		host.InvalidateToolVisual ();
		return true;
	}

	public bool PointerReleased (CanvasToolPointerContext context)
	{
		if (IsActive) {
			Complete (context.Host.Surface, context.Host.BeginStroke, context.Host.CommitStroke);
			context.Host.MarkCanvasChanged (true);
			return true;
		}
		if (nub_drag == NubDrag.None)
			return false;
		nub_drag = NubDrag.None;
		drag_snapshot = null;
		context.Host.MarkCanvasChanged (true);
		return true;
	}

	public void CancelPointerAction () => Cancel ();

	public void Begin (PointD point, ColorBgra primaryColor, ColorBgra secondaryColor, bool reverse)
	{
		start = end = point;
		primary = primaryColor;
		secondary = secondaryColor;
		reversed = reverse;
		IsActive = true;
	}

	public void Update (PointD point)
	{
		if (IsActive)
			end = point;
	}

	/// <summary>
	/// Snaps the direction from <paramref name="origin"/> to
	/// <paramref name="point"/> to the nearest <paramref name="degrees"/>
	/// increment, preserving the distance (Paint.NET Shift behavior).
	/// </summary>
	public static PointD ConstrainAngle (PointD origin, PointD point, double degrees = 15)
	{
		double dx = point.X - origin.X, dy = point.Y - origin.Y;
		double length = Math.Sqrt (dx * dx + dy * dy);
		if (length == 0)
			return point;
		double step = degrees * Math.PI / 180;
		double angle = Math.Round (Math.Atan2 (dy, dx) / step) * step;
		return new PointD (origin.X + Math.Cos (angle) * length, origin.Y + Math.Sin (angle) * length);
	}

	private static double Distance (PointD a, PointD b)
	{
		double dx = a.X - b.X, dy = a.Y - b.Y;
		return Math.Sqrt (dx * dx + dy * dy);
	}

	private static double DistanceToSegment (PointD p, PointD a, PointD b)
	{
		double dx = b.X - a.X, dy = b.Y - a.Y;
		double lengthSq = dx * dx + dy * dy;
		if (lengthSq == 0)
			return Distance (p, a);
		double t = Math.Clamp (((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSq, 0, 1);
		return Distance (p, new PointD (a.X + t * dx, a.Y + t * dy));
	}

	public void DrawGuide (DrawingContext context, Point origin, double scale, double thickness)
	{
		if (!IsActive && !HasCommitted)
			return;
		PointD first = IsActive ? start : CommittedStart;
		PointD last = IsActive ? end : CommittedEnd;
		var pen = new Pen (new SolidColorBrush (global::Avalonia.Media.Color.FromArgb (primary.A, primary.R, primary.G, primary.B)), thickness);
		Point firstView = origin + new Vector (first.X * scale, first.Y * scale);
		Point lastView = origin + new Vector (last.X * scale, last.Y * scale);
		context.DrawLine (pen, firstView, lastView);
		double radius = Math.Max (3, thickness);
		foreach (Point center in new[] { firstView, lastView }) {
			context.DrawEllipse (Brushes.White, new Pen (Brushes.Black, 1), center, radius, radius);
		}
	}

	public bool Complete (IImageSurface? surface, Action beginStroke, Action commitStroke)
	{
		if (!IsActive || surface is null)
			return false;
		beginStroke ();
		ApplyClipped (surface, start, end);
		IsActive = false;
		CommittedStart = start;
		CommittedEnd = end;
		HasCommitted = true;
		commitStroke ();
		return true;
	}

	public void Cancel ()
	{
		if (nub_drag != NubDrag.None) {
			CommittedStart = move_start;
			CommittedEnd = move_end;
		}
		IsActive = false;
		nub_drag = NubDrag.None;
		drag_snapshot = null;
	}

	private void ApplyClipped (IImageSurface surface, PointD from, PointD to)
	{
		ColorBgra[]? before = selection_mask is null ? null : surface.GetReadOnlyPixelData ().ToArray ();
		Apply (surface, from, to, primary, secondary, reversed);
		if (before is null) return;
		Span<ColorBgra> pixels = surface.GetPixelData ();
		for (int i = 0; i < pixels.Length; i++)
			if (!selection_mask![i]) pixels[i] = before[i];
	}

	public static void Apply (IImageSurface surface, PointD from, PointD to,
		ColorBgra primaryColor, ColorBgra secondaryColor, bool reverse)
		=> RasterAlgorithms.ApplyLinearGradient (surface, from, to, primaryColor, secondaryColor, reverse);
}
