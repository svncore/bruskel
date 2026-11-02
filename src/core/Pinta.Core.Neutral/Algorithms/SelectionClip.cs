using System;
using System.Collections;

namespace Pinta.Core;

/// <summary>Immutable image-space selection captured when an edit starts.</summary>
public sealed class SelectionClip
{
	private readonly BitArray mask;
	public int Width { get; }
	public int Height { get; }
	public SelectionClip (bool[] pixels, int width, int height)
	{
		if (width <= 0 || height <= 0 || pixels.Length != checked (width * height))
			throw new ArgumentException ("Selection dimensions do not match the mask.");
		mask = new BitArray (pixels);
		Width = width;
		Height = height;
	}
	public bool Contains (int x, int y) => (uint) x < Width && (uint) y < Height && mask[y * Width + x];
	public void RestoreOutside (IImageSurface surface, ReadOnlySpan<ColorBgra> before)
	{
		Span<ColorBgra> pixels = surface.GetPixelData ();
		for (int y = 0; y < surface.Height; y++)
			for (int x = 0; x < surface.Width; x++)
				if (!Contains (x, y)) pixels[y * surface.Width + x] = before[y * surface.Width + x];
	}
	public string Encode ()
	{
		byte[] bytes = new byte[(mask.Length + 7) / 8];
		mask.CopyTo (bytes, 0);
		return Convert.ToBase64String (bytes);
	}
	public static SelectionClip Decode (string value, int width, int height)
	{
		byte[] bytes = Convert.FromBase64String (value);
		int length = checked (width * height);
		if (width <= 0 || height <= 0 || bytes.Length != (length + 7L) / 8)
			throw new ArgumentException ("Invalid selection clip.");
		var bits = new BitArray (bytes);
		bool[] pixels = new bool[length];
		for (int i = 0; i < length; i++) pixels[i] = bits[i];
		return new SelectionClip (pixels, width, height);
	}
}
