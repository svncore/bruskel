namespace Bruskel.Core;

/// <summary>Configuration supplied by a platform composition root before BruskelCore is first used.</summary>
public static class BruskelCoreConfiguration
{
	public static IImageFormatProvider? ImageFormatProvider { get; set; }
}
