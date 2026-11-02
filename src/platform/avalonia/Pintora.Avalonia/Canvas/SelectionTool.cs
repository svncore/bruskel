using System;
using System.Collections.Generic;
using Avalonia;
using Pinta.Core;
using Pintora.Avalonia.Document;
using Pintora.Avalonia.Editor.Annotations;

namespace Pintora.Avalonia.Canvas;

internal sealed class SelectionTool : ICanvasTool
{
	public CanvasHost.CanvasTool Kind { get; private set; }

	public SelectionTool (CanvasHost.CanvasTool kind = CanvasHost.CanvasTool.Selection) => Kind = kind;

	internal void SetSelectionKind (CanvasHost.CanvasTool kind)
	{
		if (kind is CanvasHost.CanvasTool.Selection or CanvasHost.CanvasTool.EllipseSelect or CanvasHost.CanvasTool.LassoSelect)
			Kind = kind;
	}

	private bool IsEllipse => Kind == CanvasHost.CanvasTool.EllipseSelect;
	private bool IsLasso => Kind == CanvasHost.CanvasTool.LassoSelect;

	private ColorBgra[]? clipboard;
	private Pinta.Core.Size clipboardSize;
	private PointD? dragStart;
	private PointD? dragEnd;
	private bool square_drag;
	private SelectionCombineMode combine_mode = SelectionCombineMode.Replace;
	private readonly List<PointD> lasso_points = new ();
	private RectangleI? rectangleSelection;
	/// <summary>Bounding box of the active selection (rectangle or mask).</summary>
	public RectangleI? Selection => SelectionMask is not null ? maskBounds : rectangleSelection;
	private RectangleI maskBounds;
	public SelectionAnnotation? Annotation { get; private set; }
	public bool IsSelecting => dragStart is not null;

	/// <summary>Pixel-accurate selection (Magic Wand); when set, <see cref="Selection"/> is its bounding box.</summary>
	public bool[]? SelectionMask { get; private set; }
	public bool HasMaskSelection => SelectionMask is not null;
	internal sealed record State (RectangleI? Rectangle, bool[]? Mask, RectangleI MaskBounds, SelectionAnnotation? Annotation);
	internal State CaptureState () => new (rectangleSelection, SelectionMask is { } mask ? (bool[]) mask.Clone () : null, maskBounds, Annotation);
	internal void RestoreState (State state)
	{
		rectangleSelection = state.Rectangle;
		SelectionMask = state.Mask is { } mask ? (bool[]) mask.Clone () : null;
		maskBounds = state.MaskBounds;
		Annotation = state.Annotation;
	}
	public bool Contains (int x, int y, int width)
		=> SelectionMask is { } mask ? mask[y * width + x]
			: Selection is not RectangleI rect || (x >= rect.X && y >= rect.Y && x < rect.X + rect.Width && y < rect.Y + rect.Height);

	public bool[]? CaptureMask (Pinta.Core.Size size)
		=> SelectionMask is { } mask ? (bool[]) mask.Clone ()
			: Selection is RectangleI rect ? FloodStencil.FromRectangle (rect, size.Width, size.Height) : null;

	public bool PointerPressed (CanvasToolPointerContext context)
	{
		combine_mode = DetermineCombineMode (context);
		BeginSelection (context.Point, context.ShiftPressed);
		return true;
	}

	private static SelectionCombineMode DetermineCombineMode (CanvasToolPointerContext context)
	{
		if (context.IsRightButton)
			return context.ControlPressed ? SelectionCombineMode.Xor : SelectionCombineMode.Subtract;
		if (context.ControlPressed)
			return SelectionCombineMode.Add;
		if (context.AltPressed)
			return SelectionCombineMode.Intersect;
		return SelectionCombineMode.Replace;
	}

	public bool PointerMoved (CanvasToolPointerContext context)
	{
		if (!IsSelecting)
			return false;
		UpdateSelection (context.Point);
		context.Host.InvalidateToolVisual ();
		return true;
	}

	public bool PointerReleased (CanvasToolPointerContext context)
	{
		if (!IsSelecting)
			return false;
		CompleteSelection (context.Host.ImageSize);
		context.Host.InvalidateToolVisual ();
		return true;
	}

	public void CancelPointerAction () => CancelSelection ();

	public void BeginSelection (PointD point, bool square = false)
	{
		dragStart = dragEnd = point;
		square_drag = square && !IsLasso;
		lasso_points.Clear ();
		lasso_points.Add (point);
	}

	public void UpdateSelection (PointD point)
	{
		if (!IsSelecting)
			return;
		dragEnd = point;
		if (IsLasso)
			lasso_points.Add (point);
	}

	public bool CompleteSelection (Pinta.Core.Size imageSize)
	{
		if (dragStart is not PointD start || dragEnd is not PointD end)
			return false;
		dragStart = dragEnd = null;
		if (IsLasso)
			return CompleteLasso (imageSize);
		if (square_drag) {
			// Shift forces a square: anchor at the drag origin and expand both
			// axes to the larger extent, preserving the drag direction.
			double side = Math.Max (Math.Abs (end.X - start.X), Math.Abs (end.Y - start.Y));
			end = new PointD (
				start.X + side * Math.Sign (end.X - start.X),
				start.Y + side * Math.Sign (end.Y - start.Y));
		}
		// Pointer coordinates describe the edges of the dragged rectangle in
		// image space. Convert those edges to pixel bounds instead of rounding
		// each endpoint independently; rounding makes the selection jump at
		// fractional zoom levels and can drop the last row/column.
		double left = Math.Min (start.X, end.X);
		double top = Math.Min (start.Y, end.Y);
		double right = Math.Max (start.X, end.X);
		double bottom = Math.Max (start.Y, end.Y);
		if (IsEllipse) {
			var pixelBounds = new RectangleI ((int) Math.Floor (left), (int) Math.Floor (top),
				(int) Math.Ceiling (right) - (int) Math.Floor (left), (int) Math.Ceiling (bottom) - (int) Math.Floor (top));
			bool[] mask = FloodStencil.FromEllipse (pixelBounds, imageSize.Width, imageSize.Height);
			if (FloodStencil.GetBounds (mask, imageSize.Width, imageSize.Height) is not RectangleI bounds)
				return false;
			if (combine_mode == SelectionCombineMode.Replace) {
				SelectionMask = mask;
				maskBounds = bounds;
				rectangleSelection = null;
			} else if (!ApplyMask (mask, imageSize.Width, imageSize.Height, combine_mode)) {
				return false;
			}
			Annotation = new SelectionAnnotation {
				Bounds = new Rect (pixelBounds.X, pixelBounds.Y, pixelBounds.Width, pixelBounds.Height),
				Kind = SelectionAnnotationKind.Ellipse,
				CombineMode = Pinta.Core.SelectionCombineMode.Replace
			};
			return true;
		}
		PointI selectionStart = new ((int) Math.Floor (left), (int) Math.Floor (top));
		PointI selectionEnd = new ((int) Math.Ceiling (right), (int) Math.Ceiling (bottom));
		if (combine_mode == SelectionCombineMode.Replace)
			return SelectRectangle (selectionStart, selectionEnd, imageSize);
		RectangleI requested = RectangleI.FromPoints (selectionStart, selectionEnd);
		int x = Math.Clamp (requested.X, 0, imageSize.Width);
		int y = Math.Clamp (requested.Y, 0, imageSize.Height);
		int rightEdge = Math.Clamp (requested.X + requested.Width, 0, imageSize.Width);
		int bottomEdge = Math.Clamp (requested.Y + requested.Height, 0, imageSize.Height);
		if (rightEdge <= x || bottomEdge <= y)
			return false;
		return ApplyMask (FloodStencil.FromRectangle (new RectangleI (x, y, rightEdge - x, bottomEdge - y), imageSize.Width, imageSize.Height),
			imageSize.Width, imageSize.Height, combine_mode);
	}

	private bool CompleteLasso (Pinta.Core.Size imageSize)
	{
		var points = new List<PointD> (lasso_points);
		lasso_points.Clear ();
		bool[] mask = FloodStencil.FromPolygon (points, imageSize.Width, imageSize.Height);
		if (FloodStencil.GetBounds (mask, imageSize.Width, imageSize.Height) is not RectangleI bounds)
			return false;
		if (combine_mode == SelectionCombineMode.Replace) {
			SelectionMask = mask;
			maskBounds = bounds;
			rectangleSelection = null;
		} else if (!ApplyMask (mask, imageSize.Width, imageSize.Height, combine_mode)) {
			return false;
		}
		Annotation = new SelectionAnnotation {
			Bounds = new Rect (bounds.X, bounds.Y, bounds.Width, bounds.Height),
			Kind = SelectionAnnotationKind.Freeform,
			CombineMode = Pinta.Core.SelectionCombineMode.Replace
		};
		return true;
	}

	public void CancelSelection ()
	{
		dragStart = dragEnd = null;
		lasso_points.Clear ();
	}

	public bool SelectRectangle (PointI start, PointI end, Pinta.Core.Size imageSize)
	{
		RectangleI requested = RectangleI.FromPoints (start, end);
		int left = Math.Clamp (requested.X, 0, imageSize.Width);
		int top = Math.Clamp (requested.Y, 0, imageSize.Height);
		int right = Math.Clamp (requested.X + requested.Width, 0, imageSize.Width);
		int bottom = Math.Clamp (requested.Y + requested.Height, 0, imageSize.Height);
		if (right <= left || bottom <= top) {
			rectangleSelection = null;
			SelectionMask = null;
			Annotation = null;
			return false;
		}
		SelectionMask = null;
		rectangleSelection = new RectangleI (left, top, right - left, bottom - top);
		Annotation = new SelectionAnnotation {
			Bounds = new Rect (left, top, right - left, bottom - top),
			Kind = SelectionAnnotationKind.Rectangle,
			CombineMode = Pinta.Core.SelectionCombineMode.Replace
		};
		return true;
	}

	public void Clear () { rectangleSelection = null; SelectionMask = null; Annotation = null; }

	/// <summary>
	/// Moves the selection outline (rectangle or mask) without touching pixels —
	/// the Move Selection tool behavior. Pixels are moved separately via
	/// <see cref="Move"/>.
	/// </summary>
	public bool Translate (PointI delta, Pinta.Core.Size imageSize)
	{
		if ((delta.X == 0 && delta.Y == 0) || Selection is null)
			return false;
		if (SelectionMask is { } mask) {
			bool[] shifted = new bool[mask.Length];
			for (int y = 0; y < imageSize.Height; y++) {
				int sourceY = y - delta.Y;
				if ((uint) sourceY >= (uint) imageSize.Height)
					continue;
				for (int x = 0; x < imageSize.Width; x++) {
					int sourceX = x - delta.X;
					if ((uint) sourceX < (uint) imageSize.Width && mask[sourceY * imageSize.Width + sourceX])
						shifted[y * imageSize.Width + x] = true;
				}
			}
			SelectionMask = shifted;
			if (FloodStencil.GetBounds (shifted, imageSize.Width, imageSize.Height) is not RectangleI bounds) {
				Clear ();
				return false;
			}
			maskBounds = bounds;
		} else if (rectangleSelection is RectangleI rect) {
			int maxX = Math.Max (0, imageSize.Width - rect.Width);
			int maxY = Math.Max (0, imageSize.Height - rect.Height);
			rectangleSelection = new RectangleI (
				Math.Clamp (rect.X + delta.X, 0, maxX),
				Math.Clamp (rect.Y + delta.Y, 0, maxY),
				rect.Width, rect.Height);
		} else {
			return false;
		}
		if (Annotation is not null && Selection is RectangleI area)
			Annotation = Annotation with { Bounds = new Rect (area.X, area.Y, area.Width, area.Height) };
		return true;
	}

	/// <summary>
	/// Applies a pixel mask as the selection, combining with the current selection
	/// according to <paramref name="mode"/> (GTK SelectionModeHandler semantics,
	/// but as boolean mask ops — no polygon clipping needed).
	/// </summary>
	public bool ApplyMask (bool[] mask, int width, int height, SelectionCombineMode mode)
	{
		ArgumentNullException.ThrowIfNull (mask);
		if (width < 0 || height < 0 || mask.Length != width * height)
			throw new ArgumentException ("Selection mask dimensions do not match the image.", nameof (mask));

		bool[] combined = mask;
		if (mode != SelectionCombineMode.Replace && SelectionMask is { } current && current.Length == mask.Length)
			combined = FloodStencil.Combine (current, mask, mode);
		else if (mode != SelectionCombineMode.Replace && SelectionMask is null && rectangleSelection is RectangleI rect)
			combined = FloodStencil.Combine (FloodStencil.FromRectangle (rect, width, height), mask, mode);

		// Compute the bounding box; an empty result clears the selection.
		RectangleI? bounds = FloodStencil.GetBounds (combined, width, height);
		if (bounds is not RectangleI selectionBounds) {
			Clear ();
			return false;
		}
		SelectionMask = combined;
		maskBounds = selectionBounds;
		Annotation = null;
		return true;
	}

	public bool Copy (IImageSurface? surface)
	{
		if (surface is null || Selection is not RectangleI area)
			return false;
		clipboardSize = new Pinta.Core.Size (area.Width, area.Height);
		clipboard = new ColorBgra[area.Width * area.Height];
		ReadOnlySpan<ColorBgra> source = surface.GetReadOnlyPixelData ();
		for (int y = 0; y < area.Height; y++)
			for (int x = 0; x < area.Width; x++)
				if (Contains (area.X + x, area.Y + y, surface.Width))
					clipboard[y * area.Width + x] = source[(area.Y + y) * surface.Width + area.X + x];
		return true;
	}

	public bool Paste (PointI destination, Func<string, AvaloniaLayer> createLayer)
	{
		if (clipboard is null || clipboardSize.Width <= 0 || clipboardSize.Height <= 0)
			return false;
		AvaloniaLayer layer = createLayer ($"Pasted Layer");
		Span<ColorBgra> target = layer.Surface.GetPixelData ();
		for (int y = 0; y < clipboardSize.Height; y++) {
			int targetY = destination.Y + y;
			if ((uint) targetY >= (uint) layer.Surface.Height) continue;
			for (int x = 0; x < clipboardSize.Width; x++) {
				int targetX = destination.X + x;
				if ((uint) targetX < (uint) layer.Surface.Width)
					target[targetY * layer.Surface.Width + targetX] = clipboard[y * clipboardSize.Width + x];
			}
		}
		return true;
	}

	public bool Move (IImageSurface? surface, PointI delta, Action beginStroke, Action commitStroke)
	{
		if (surface is null || Selection is not RectangleI area || (delta.X == 0 && delta.Y == 0) || !Copy (surface))
			return false;
		PointI destination = new (area.X + delta.X, area.Y + delta.Y);
		int left = Math.Max (0, destination.X), top = Math.Max (0, destination.Y);
		var clipped = new RectangleI (left, top,
			Math.Min (destination.X + area.Width, surface.Width) - left,
			Math.Min (destination.Y + area.Height, surface.Height) - top);
		if (clipped.Width <= 0 || clipped.Height <= 0)
			return false;
		beginStroke ();
		Span<ColorBgra> pixels = surface.GetPixelData ();
		bool[]? oldMask = SelectionMask;
		for (int y = 0; y < area.Height; y++)
			for (int x = 0; x < area.Width; x++)
				if (Contains (area.X + x, area.Y + y, surface.Width))
					pixels[(area.Y + y) * surface.Width + area.X + x] = ColorBgra.Zero;
		for (int y = 0; y < clipped.Height; y++)
			for (int x = 0; x < clipped.Width; x++) {
				int sourceX = clipped.X + x - destination.X, sourceY = clipped.Y + y - destination.Y;
				if (oldMask is not null && !oldMask[(area.Y + sourceY) * surface.Width + area.X + sourceX]) continue;
				pixels[(clipped.Y + y) * surface.Width + clipped.X + x] = clipboard![sourceY * area.Width + sourceX];
			}
		if (oldMask is not null)
			Translate (delta, new Pinta.Core.Size (surface.Width, surface.Height));
		else
			rectangleSelection = clipped;
		if (Annotation is not null)
			Annotation = Annotation with { Bounds = new Rect (clipped.X, clipped.Y, clipped.Width, clipped.Height) };
		commitStroke ();
		return true;
	}

	public bool FlipHorizontal (IImageSurface? surface, Action beginStroke, Action commitStroke)
	{
		if (SelectionMask is not null && surface is not null && Selection is RectangleI bounds)
			return TransformMask (surface, (x, y) => (bounds.Width - 1 - x, y), beginStroke, commitStroke);
		if (surface is null || Selection is not RectangleI area || area.Width < 2) return false;
		beginStroke ();
		Span<ColorBgra> pixels = surface.GetPixelData ();
		for (int y = 0; y < area.Height; y++)
			for (int x = 0; x < area.Width / 2; x++) {
				int left = (area.Y + y) * surface.Width + area.X + x;
				int right = (area.Y + y) * surface.Width + area.X + area.Width - x - 1;
				(pixels[left], pixels[right]) = (pixels[right], pixels[left]);
			}
		commitStroke ();
		return true;
	}

	public bool FlipVertical (IImageSurface? surface, Action beginStroke, Action commitStroke)
	{
		if (SelectionMask is not null && surface is not null && Selection is RectangleI bounds)
			return TransformMask (surface, (x, y) => (x, bounds.Height - 1 - y), beginStroke, commitStroke);
		if (surface is null || Selection is not RectangleI area || area.Height < 2) return false;
		beginStroke ();
		Span<ColorBgra> pixels = surface.GetPixelData ();
		for (int y = 0; y < area.Height / 2; y++)
			for (int x = 0; x < area.Width; x++) {
				int top = (area.Y + y) * surface.Width + area.X + x;
				int bottom = (area.Y + area.Height - y - 1) * surface.Width + area.X + x;
				(pixels[top], pixels[bottom]) = (pixels[bottom], pixels[top]);
			}
		commitStroke ();
		return true;
	}

	public bool RotateClockwise (IImageSurface? surface, Action beginStroke, Action commitStroke)
	{
		if (SelectionMask is not null && surface is not null && Selection is RectangleI bounds && bounds.Width == bounds.Height)
			return TransformMask (surface, (x, y) => (bounds.Height - 1 - y, x), beginStroke, commitStroke);
		if (surface is null || Selection is not RectangleI area || area.Width != area.Height || area.Width < 2) return false;
		int size = area.Width;
		ColorBgra[] source = new ColorBgra[size * size];
		for (int y = 0; y < size; y++)
			for (int x = 0; x < size; x++)
				source[y * size + x] = surface.GetReadOnlyPixelData ()[(area.Y + y) * surface.Width + area.X + x];
		beginStroke ();
		Span<ColorBgra> pixels = surface.GetPixelData ();
		for (int y = 0; y < size; y++)
			for (int x = 0; x < size; x++)
				pixels[(area.Y + y) * surface.Width + area.X + x] = source[(size - 1 - x) * size + y];
		commitStroke ();
		return true;
	}

	private bool TransformMask (IImageSurface surface, Func<int, int, (int X, int Y)> transform, Action beginStroke, Action commitStroke)
	{
		if (Selection is not RectangleI area || SelectionMask is not { } mask) return false;
		ColorBgra[] source = surface.GetReadOnlyPixelData ().ToArray ();
		bool[] next = new bool[mask.Length];
		beginStroke ();
		Span<ColorBgra> pixels = surface.GetPixelData ();
		for (int i = 0; i < mask.Length; i++) if (mask[i]) pixels[i] = ColorBgra.Zero;
		for (int y = 0; y < area.Height; y++)
			for (int x = 0; x < area.Width; x++) {
				int sourceIndex = (area.Y + y) * surface.Width + area.X + x;
				if (!mask[sourceIndex]) continue;
				(int tx, int ty) = transform (x, y);
				int targetIndex = (area.Y + ty) * surface.Width + area.X + tx;
				pixels[targetIndex] = source[sourceIndex];
				next[targetIndex] = true;
			}
		ApplyMask (next, surface.Width, surface.Height, SelectionCombineMode.Replace);
		commitStroke ();
		return true;
	}
}
