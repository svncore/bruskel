using SkiaSharp;

namespace Bruskel.Graphics.Skia;

/// <summary>Skia-backed implementation of the neutral image surface factory.</summary>
public sealed class SkiaImageSurfaceFactory : Bruskel.Core.IImageSurfaceFactory
{
	public Bruskel.Core.IImageSurface Create (int width, int height)
	{
		if (width < 0) throw new ArgumentOutOfRangeException (nameof (width));
		if (height < 0) throw new ArgumentOutOfRangeException (nameof (height));
		return new SkiaImageSurface (new SKBitmap (width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
	}
}
