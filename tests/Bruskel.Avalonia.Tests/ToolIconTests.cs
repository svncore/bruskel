using NUnit.Framework;
using Bruskel.Avalonia.Canvas;

namespace Bruskel.Avalonia.Tests;

[TestFixture]
internal sealed class ToolIconTests
{
	[Test]
	public void AllToolIconsCanBeLoadedFromPublishedResources ()
	{
		foreach (var descriptor in CanvasToolFactory.Descriptors) {
			string data = ToolIcons.LoadPath (descriptor.Tool, "M0 0H24V24H0Z");
			using var path = SkiaSharp.SKPath.ParseSvgPathData (data);
			Assert.That (path, Is.Not.Null, descriptor.DisplayName);
			Assert.That (path!.IsEmpty, Is.False, descriptor.DisplayName);
		}
	}
}
