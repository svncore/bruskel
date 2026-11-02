using NUnit.Framework;
using Bruskel.Core;
using Bruskel.Avalonia.Canvas;
using Bruskel.Avalonia.Imaging;

namespace Bruskel.Avalonia.Tests;

[TestFixture]
internal sealed class ShapeColorTests
{
	[Test]
	public void ActiveShapeFollowsPaletteAndUndoRestoresItsColors ()
	{
		using var surface = new MemoryImageSurface (12, 12);
		using var canvas = new CanvasHost (new Bruskel.Core.Size (12, 12), surface) {
			ActiveTool = CanvasHost.CanvasTool.Shapes, ShapeAntialias = false,
			ShapeOutlineWidth = 1, FillStyle = CanvasHost.ShapeFillStyle.FillAndOutline,
			BrushColor = ColorBgra.Red, SecondaryBrushColor = ColorBgra.White
		};
		canvas.PaintCanvasRectangle (new PointD (2, 2), new PointD (9, 9), ColorBgra.Red);
		canvas.BrushColor = ColorBgra.Blue;
		Assert.That (surface.GetReadOnlyPixelData ()[2 * 12 + 2], Is.EqualTo (ColorBgra.Blue));
		canvas.SecondaryBrushColor = ColorBgra.Green;
		Assert.That (surface.GetReadOnlyPixelData ()[5 * 12 + 5], Is.EqualTo (ColorBgra.Green));
		canvas.UndoLastStroke ();
		Assert.That (surface.GetReadOnlyPixelData ()[5 * 12 + 5], Is.EqualTo (ColorBgra.White));
		canvas.FinishShape ();
		ColorBgra[] before = surface.GetReadOnlyPixelData ().ToArray ();
		canvas.BrushColor = ColorBgra.Black;
		Assert.That (surface.GetReadOnlyPixelData ().ToArray (), Is.EqualTo (before));
	}

	[Test]
	public void RightButtonShapeKeepsReversedPaletteRoles ()
	{
		using var surface = new MemoryImageSurface (12, 12);
		using var canvas = new CanvasHost (new Bruskel.Core.Size (12, 12), surface) {
			ActiveTool = CanvasHost.CanvasTool.Shapes, ShapeAntialias = false,
			ShapeOutlineWidth = 1, FillStyle = CanvasHost.ShapeFillStyle.FillAndOutline,
			BrushColor = ColorBgra.Red, SecondaryBrushColor = ColorBgra.White
		};
		var tool = CanvasToolFactory.CreateTools ().Get<ShapeTool> (CanvasHost.CanvasTool.Shapes);
		CanvasToolPointerContext context = new (canvas, default, new PointD (2, 2), default, ColorBgra.White, true, false);
		tool.PointerPressed (context);
		tool.PointerMoved (context with { Point = new PointD (9, 9) });
		tool.PointerReleased (context with { Point = new PointD (9, 9) });
		tool.UpdateColors (canvas.ActiveLayer, surface, ColorBgra.Blue, ColorBgra.Green, canvas.BeginStroke, canvas.CommitStroke);
		Assert.That (surface.GetReadOnlyPixelData ()[2 * 12 + 2], Is.EqualTo (ColorBgra.Green));
		Assert.That (surface.GetReadOnlyPixelData ()[5 * 12 + 5], Is.EqualTo (ColorBgra.Blue));
	}
}
