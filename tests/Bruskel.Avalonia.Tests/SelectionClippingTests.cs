using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using Bruskel.Core;
using Bruskel.Avalonia.Canvas;
using Bruskel.Avalonia.Editor.Annotations;
using Bruskel.Avalonia.Imaging;

namespace Bruskel.Avalonia.Tests;

[TestFixture]
internal sealed class SelectionClippingTests
{
	[TestCase (false)]
	[TestCase (true)]
	public void FillCannotCrossDisconnectedSelection (bool global)
	{
		using var surface = new MemoryImageSurface (5, 1);
		surface.Fill (ColorBgra.White);
		bool[] allowed = [true, true, false, true, true];
		var fill = new FillTool ();
		fill.Fill (surface, new PointD (0, 0), ColorBgra.Red, 0, global, () => { }, () => { }, allowed);
		Assert.That (surface.GetReadOnlyPixelData ()[2], Is.EqualTo (ColorBgra.White));
		Assert.That (surface.GetReadOnlyPixelData ()[4], Is.EqualTo (global ? ColorBgra.Red : ColorBgra.White));
	}

	[Test]
	public void GradientAndShapeRespectSelection ()
	{
		using var initial = new MemoryImageSurface (8, 8);
		using var canvas = new CanvasHost (new Bruskel.Core.Size (8, 8), initial);
		canvas.SelectRectangle (new PointI (2, 2), new PointI (5, 5));
		canvas.PaintGradient (new PointD (0, 0), new PointD (8, 0));
		Assert.That (canvas.Surface!.GetReadOnlyPixelData ()[0], Is.EqualTo (ColorBgra.Zero));
		Assert.That (canvas.Surface.GetReadOnlyPixelData ()[3 * 8 + 3].A, Is.GreaterThan (0));
		canvas.PaintCanvasRectangle (new PointD (0, 0), new PointD (7, 7), ColorBgra.Red);
		Assert.That (canvas.Surface.GetReadOnlyPixelData ()[0], Is.EqualTo (ColorBgra.Zero));
	}

	[Test]
	public void MoveMaskLeavesUnselectedPixelsAndMovesMask ()
	{
		using var surface = new MemoryImageSurface (8, 4);
		surface.Fill (ColorBgra.White);
		bool[] mask = new bool[32];
		mask[1 * 8 + 1] = mask[2 * 8 + 2] = true;
		surface.GetPixelData ()[9] = ColorBgra.Red;
		var selection = new SelectionTool ();
		selection.ApplyMask (mask, 8, 4, SelectionCombineMode.Replace);
		selection.Move (surface, new PointI (3, 0), () => { }, () => { });
		Assert.That (surface.GetReadOnlyPixelData ()[10], Is.EqualTo (ColorBgra.White));
		Assert.That (surface.GetReadOnlyPixelData ()[9], Is.EqualTo (ColorBgra.Zero));
		Assert.That (surface.GetReadOnlyPixelData ()[12], Is.EqualTo (ColorBgra.Red));
		Assert.That (selection.SelectionMask![12], Is.True);
		Assert.That (selection.SelectionMask[9], Is.False);
	}

	[Test]
	public void EffectClipSurvivesRenderAndOraRoundTrip ()
	{
		using var initial = new MemoryImageSurface (8, 8);
		using var canvas = new CanvasHost (new Bruskel.Core.Size (8, 8), initial);
		canvas.SelectRectangle (new PointI (2, 2), new PointI (5, 5));
		var effect = new RegionEffectAnnotation {
			Bounds = new global::Avalonia.Rect (0, 0, 8, 8), Effect = RegionEffectKind.Highlight,
			Clip = canvas.CaptureSelectionClip ()
		};
		canvas.Layers[0].Annotations.Add (effect);
		string path = Path.Combine (Path.GetTempPath (), Guid.NewGuid () + ".ora");
		try {
			using (var archive = ZipFile.Open (path, ZipArchiveMode.Create))
				typeof (CanvasHost).GetMethod ("SaveRasterAnnotations", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke (canvas, [archive]);
			using var restoredSurface = new MemoryImageSurface (8, 8);
			using var restored = new CanvasHost (new Bruskel.Core.Size (8, 8), restoredSurface);
			using (var archive = ZipFile.OpenRead (path))
				typeof (CanvasHost).GetMethod ("LoadRasterAnnotations", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke (restored, [archive]);
			var loaded = restored.Layers[0].Annotations.Items.OfType<RegionEffectAnnotation> ().Single ();
			using var surface = new MemoryImageSurface (8, 8);
			surface.Fill (ColorBgra.White);
			AnnotationRenderer.Render (surface, loaded);
			Assert.That (surface.GetReadOnlyPixelData ()[0], Is.EqualTo (ColorBgra.White));
			Assert.That (surface.GetReadOnlyPixelData ()[27], Is.Not.EqualTo (ColorBgra.White));
		} finally { File.Delete (path); }
	}

	[Test]
	public void CloneStampClipsDestinationButCanSampleOutsideSelection ()
	{
		using var surface = new MemoryImageSurface (8, 8);
		surface.Fill (ColorBgra.White);
		surface.GetPixelData ()[0] = ColorBgra.Red;
		using var canvas = new CanvasHost (new Bruskel.Core.Size (8, 8), surface) { BrushSize = 3 };
		canvas.SelectRectangle (new PointI (4, 4), new PointI (5, 5));
		var tool = new CloneStampTool ();
		CanvasToolPointerContext context = new (canvas, default, new PointD (0, 0), default, ColorBgra.Black, false, false, true);
		tool.PointerPressed (context);
		tool.PointerPressed (context with { Point = new PointD (4, 4), ControlPressed = false });
		Assert.That (surface.GetReadOnlyPixelData ()[36], Is.EqualTo (ColorBgra.Red));
		Assert.That (surface.GetReadOnlyPixelData ()[35], Is.EqualTo (ColorBgra.White));
	}

	[Test]
	public void MoveOutlineCanReturnFromOutsideCanvasAndCancel ()
	{
		using var surface = new MemoryImageSurface (8, 8);
		using var canvas = new CanvasHost (new Bruskel.Core.Size (8, 8), surface);
		bool[] mask = new bool[64]; mask[9] = true;
		canvas.ApplySelectionMask (mask, SelectionCombineMode.Replace);
		var tool = new MoveTool (CanvasHost.CanvasTool.MoveSelection);
		CanvasToolPointerContext context = new (canvas, default, new PointD (1, 1), default, ColorBgra.Black, false, false);
		tool.PointerPressed (context);
		tool.PointerMoved (context with { Point = new PointD (100, 100) });
		tool.PointerMoved (context with { Point = new PointD (3, 3) });
		Assert.That (canvas.Selection, Is.EqualTo (new RectangleI (3, 3, 1, 1)));
		tool.CancelPointerAction ();
		Assert.That (canvas.Selection, Is.EqualTo (new RectangleI (1, 1, 1, 1)));
	}

	[Test]
	public void MaskedFlipMovesOnlySelectedPixels ()
	{
		using var surface = new MemoryImageSurface (5, 5);
		surface.Fill (ColorBgra.White);
		surface.GetPixelData ()[6] = ColorBgra.Red;
		bool[] mask = new bool[25]; mask[6] = mask[18] = true;
		var tool = new SelectionTool ();
		tool.ApplyMask (mask, 5, 5, SelectionCombineMode.Replace);
		tool.FlipHorizontal (surface, () => { }, () => { });
		Assert.That (surface.GetReadOnlyPixelData ()[7], Is.EqualTo (ColorBgra.White));
		Assert.That (surface.GetReadOnlyPixelData ()[8], Is.EqualTo (ColorBgra.Red));
		Assert.That (tool.SelectionMask![8], Is.True);
		Assert.That (tool.SelectionMask[6], Is.False);
	}
}
