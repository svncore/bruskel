/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See third-party/paint.net/LICENSE.md for full licensing and attribution details.             //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace Pinta.Core;

/// <summary>
/// Select-by-color stencil algorithms, ported from the GTK FloodTool
/// (Pinta.Tools/Tools/FloodTool.cs → CairoExtensions.FillStencilFromPoint / FillStencilByColor).
/// Platform-neutral: operates on any <see cref="IImageSurface"/> and uses the GTK/PDN
/// tolerance math <see cref="ColorBgra.ColorsWithinTolerance"/>.
/// GUI tools (Avalonia MagicWandTool, GTK MagicWandTool) are thin adapters over these.
/// </summary>
public static class FloodStencil
{
	/// <summary>
	/// Contiguous flood from the seed point: only neighbors within tolerance of the seed
	/// color are marked (GTK FillStencilFromPoint).
	/// </summary>
	public static bool[] FromPoint (IImageSurface surface, int startX, int startY, int tolerance, bool[]? allowed = null)
	{
		ArgumentNullException.ThrowIfNull (surface);
		int width = surface.Width, height = surface.Height;
		ValidateAllowed (allowed, width, height);
		if ((uint) startX >= (uint) width || (uint) startY >= (uint) height)
			throw new ArgumentOutOfRangeException (nameof (startX), "Seed point is outside the surface.");
		if (allowed is not null && !allowed[startY * width + startX])
			return new bool[width * height];

		ReadOnlySpan<ColorBgra> pixels = surface.GetReadOnlyPixelData ();
		ColorBgra cmp = pixels[startY * width + startX];

		var mask = new bool[width * height];
		var queue = new Queue<(int X, int Y)> ();
		queue.Enqueue ((startX, startY));
		while (queue.Count > 0) {
			(int x, int y) = queue.Dequeue ();
			if ((uint) x >= (uint) width || (uint) y >= (uint) height)
				continue;
			int index = y * width + x;
			if (mask[index] || (allowed is not null && !allowed[index]) || !ColorBgra.ColorsWithinTolerance (cmp, pixels[index], tolerance))
				continue;
			mask[index] = true;
			queue.Enqueue ((x + 1, y));
			queue.Enqueue ((x - 1, y));
			queue.Enqueue ((x, y + 1));
			queue.Enqueue ((x, y - 1));
		}
		return mask;
	}

	/// <summary>
	/// Global scan: ALL pixels within tolerance of the seed color are marked, wherever they
	/// are on the surface (GTK FillStencilByColor). Returns null when nothing matches.
	/// </summary>
	public static bool[]? ByColor (IImageSurface surface, int startX, int startY, int tolerance, bool[]? allowed = null)
	{
		ArgumentNullException.ThrowIfNull (surface);
		int width = surface.Width, height = surface.Height;
		ValidateAllowed (allowed, width, height);
		if ((uint) startX >= (uint) width || (uint) startY >= (uint) height)
			throw new ArgumentOutOfRangeException (nameof (startX), "Seed point is outside the surface.");

		ReadOnlySpan<ColorBgra> pixels = surface.GetReadOnlyPixelData ();
		ColorBgra cmp = pixels[startY * width + startX];

		var mask = new bool[width * height];
		bool any = false;
		for (int i = 0; i < mask.Length; i++) {
			if ((allowed is null || allowed[i]) && ColorBgra.ColorsWithinTolerance (cmp, pixels[i], tolerance)) {
				mask[i] = true;
				any = true;
			}
		}
		return any ? mask : null;
	}

	private static void ValidateAllowed (bool[]? allowed, int width, int height)
	{
		if (allowed is not null && allowed.Length != checked (width * height))
			throw new ArgumentException ("Allowed mask dimensions do not match the image.", nameof (allowed));
	}

	/// <summary>Converts a 0–100 UI tolerance percentage to the GTK stencil scale (t²·256/100²).</summary>
	public static int PercentToTolerance (int percent)
		=> (int) (Math.Clamp (percent, 0, 100) * Math.Clamp (percent, 0, 100) * 256 / 10000.0);

	/// <summary>
	/// Combines two masks of equal length per the selection combine mode
	/// (GTK SelectionModeHandler semantics as boolean ops — no polygon clipping needed).
	/// </summary>
	public static bool[] Combine (bool[] current, bool[] mask, SelectionCombineMode mode)
	{
		ArgumentNullException.ThrowIfNull (current);
		ArgumentNullException.ThrowIfNull (mask);
		if (current.Length != mask.Length)
			throw new ArgumentException ("Masks must have equal length.", nameof (mask));

		var result = new bool[current.Length];
		for (int i = 0; i < current.Length; i++)
			result[i] = mode switch {
				SelectionCombineMode.Add => current[i] || mask[i],
				SelectionCombineMode.Subtract => current[i] && !mask[i],
				SelectionCombineMode.Intersect => current[i] && mask[i],
				SelectionCombineMode.Xor => current[i] ^ mask[i],
				_ => mask[i],
			};
		return result;
	}

	/// <summary>Builds a full-size mask from a rectangle (used to combine rectangle and wand selections).</summary>
	public static bool[] FromRectangle (RectangleI rect, int width, int height)
	{
		var mask = new bool[width * height];
		for (int y = Math.Max (0, rect.Y); y < Math.Min (height, rect.Y + rect.Height); y++)
			for (int x = Math.Max (0, rect.X); x < Math.Min (width, rect.X + rect.Width); x++)
				mask[y * width + x] = true;
		return mask;
	}

	/// <summary>Builds a full-size elliptical mask inside the bounding box (Ellipse Select).</summary>
	public static bool[] FromEllipse (RectangleI bounds, int width, int height)
	{
		var mask = new bool[width * height];
		if (bounds.Width <= 0 || bounds.Height <= 0)
			return mask;
		double radiusX = bounds.Width / 2.0;
		double radiusY = bounds.Height / 2.0;
		double centerX = bounds.X + radiusX;
		double centerY = bounds.Y + radiusY;
		for (int y = Math.Max (0, bounds.Y); y < Math.Min (height, bounds.Y + bounds.Height); y++)
			for (int x = Math.Max (0, bounds.X); x < Math.Min (width, bounds.X + bounds.Width); x++) {
				double dx = (x + 0.5 - centerX) / radiusX;
				double dy = (y + 0.5 - centerY) / radiusY;
				if (dx * dx + dy * dy <= 1.0)
					mask[y * width + x] = true;
			}
		return mask;
	}

	/// <summary>Rasterizes a closed polygon (even-odd rule) into a full-size mask (Lasso Select).</summary>
	public static bool[] FromPolygon (System.Collections.Generic.IReadOnlyList<PointD> points, int width, int height)
	{
		var mask = new bool[width * height];
		if (points.Count < 3)
			return mask;
		double minY = points[0].Y;
		double maxY = points[0].Y;
		foreach (PointD point in points) {
			minY = Math.Min (minY, point.Y);
			maxY = Math.Max (maxY, point.Y);
		}
		for (int y = Math.Max (0, (int) Math.Floor (minY)); y <= Math.Min (height - 1, (int) Math.Floor (maxY)); y++) {
			double scanY = y + 0.5;
			var crossings = new System.Collections.Generic.List<double> ();
			for (int i = 0; i < points.Count; i++) {
				PointD a = points[i];
				PointD b = points[(i + 1) % points.Count];
				if ((a.Y <= scanY && b.Y > scanY) || (b.Y <= scanY && a.Y > scanY)) {
					double t = (scanY - a.Y) / (b.Y - a.Y);
					crossings.Add (a.X + t * (b.X - a.X));
				}
			}
			crossings.Sort ();
			for (int i = 0; i + 1 < crossings.Count; i += 2) {
				int xStart = Math.Clamp ((int) Math.Ceiling (crossings[i] - 0.5), 0, width);
				int xEnd = Math.Clamp ((int) Math.Ceiling (crossings[i + 1] - 0.5), 0, width);
				for (int x = xStart; x < xEnd; x++)
					mask[y * width + x] = true;
			}
		}
		return mask;
	}

	/// <summary>Bounding box of a mask, or null when the mask is empty.</summary>
	public static RectangleI? GetBounds (bool[] mask, int width, int height)
	{
		int left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
		for (int y = 0; y < height; y++)
			for (int x = 0; x < width; x++) {
				if (!mask[y * width + x])
					continue;
				if (x < left) left = x;
				if (x > right) right = x;
				if (y < top) top = y;
				if (y > bottom) bottom = y;
			}
		return right < left || bottom < top ? null : new RectangleI (left, top, right - left + 1, bottom - top + 1);
	}
}
