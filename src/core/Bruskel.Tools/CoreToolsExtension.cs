// 
// CoreToolsExtension.cs
//  
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
// 
// Copyright (c) 2011 Jonathan Pobst
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Bruskel.Core;

[assembly: Mono.Addins.Addin ("DefaultTools", BruskelCore.ApplicationVersion, Category = "Core")]
[assembly: Mono.Addins.AddinName ("Default Tools")]
[assembly: Mono.Addins.AddinDescription ("The default tools and brushes that ship with Bruskel")]
[assembly: Mono.Addins.AddinDependency ("Bruskel", BruskelCore.ApplicationVersion)]
[assembly: Mono.Addins.AddinFlags (Mono.Addins.Description.AddinFlags.Hidden | Mono.Addins.Description.AddinFlags.CantUninstall)]

namespace Bruskel.Tools;

[Mono.Addins.Extension]
public sealed class CoreToolsExtension : IExtension
{
	#region IExtension Members
	public void Initialize ()
	{
		IServiceProvider services = BruskelCore.Services;

		BruskelCore.PaintBrushes.AddPaintBrush (new Brushes.CircleBrush ());
		BruskelCore.PaintBrushes.AddPaintBrush (new Brushes.GridBrush ());
		BruskelCore.PaintBrushes.AddPaintBrush (new Brushes.PlainBrush (BruskelCore.Workspace));
		BruskelCore.PaintBrushes.AddPaintBrush (new Brushes.SplatterBrush (services.GetService<ISettingsService> ()));
		BruskelCore.PaintBrushes.AddPaintBrush (new Brushes.SlashBrush (services.GetService<ISettingsService> (), services.GetService<IWorkspaceService> ()));
		BruskelCore.PaintBrushes.AddPaintBrush (new Brushes.SquaresBrush ());

		BruskelCore.Tools.AddTool (new MoveSelectedTool (services));
		BruskelCore.Tools.AddTool (new MoveSelectionTool (services));
		BruskelCore.Tools.AddTool (new ZoomTool (services));
		BruskelCore.Tools.AddTool (new PanTool (services));
		BruskelCore.Tools.AddTool (new RectangleSelectTool (services));
		BruskelCore.Tools.AddTool (new EllipseSelectTool (services));
		BruskelCore.Tools.AddTool (new LassoSelectTool (services));
		BruskelCore.Tools.AddTool (new MagicWandTool (services));
		BruskelCore.Tools.AddTool (new PaintBrushTool (services));
		BruskelCore.Tools.AddTool (new PencilTool (services));
		BruskelCore.Tools.AddTool (new EraserTool (services));
		BruskelCore.Tools.AddTool (new PaintBucketTool (services));
		BruskelCore.Tools.AddTool (new GradientTool (services));
		BruskelCore.Tools.AddTool (new ColorPickerTool (services));
		BruskelCore.Tools.AddTool (new TextTool (services));
		BruskelCore.Tools.AddTool (new LineCurveTool (services));
		BruskelCore.Tools.AddTool (new RectangleTool (services));
		BruskelCore.Tools.AddTool (new RoundedRectangleTool (services));
		BruskelCore.Tools.AddTool (new EllipseTool (services));
		BruskelCore.Tools.AddTool (new FreeformShapeTool (services));
		BruskelCore.Tools.AddTool (new CloneStampTool (services));
		BruskelCore.Tools.AddTool (new RecolorTool (services));
	}

	public void Uninitialize ()
	{
		BruskelCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (Brushes.CircleBrush));
		BruskelCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (Brushes.GridBrush));
		BruskelCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (Brushes.PlainBrush));
		BruskelCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (Brushes.SplatterBrush));
		BruskelCore.PaintBrushes.RemoveInstanceOfPaintBrush (typeof (Brushes.SquaresBrush));

		BruskelCore.Tools.RemoveInstanceOfTool<MoveSelectedTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<MoveSelectionTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<ZoomTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<PanTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<RectangleSelectTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<EllipseSelectTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<LassoSelectTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<MagicWandTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<PaintBrushTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<PencilTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<EraserTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<PaintBucketTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<GradientTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<ColorPickerTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<TextTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<LineCurveTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<RectangleTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<RoundedRectangleTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<EllipseTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<FreeformShapeTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<CloneStampTool> ();
		BruskelCore.Tools.RemoveInstanceOfTool<RecolorTool> ();
	}
	#endregion
}
