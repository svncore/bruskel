using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Bruskel.Avalonia.Canvas;

internal static class ToolIcons
{
	private static readonly Dictionary<string, string> cache = new ();
	internal static string LoadPath (CanvasHost.CanvasTool tool, string fallback)
	{
		string? name = tool switch {
			CanvasHost.CanvasTool.Selection => "select-rectangle",
			CanvasHost.CanvasTool.EllipseSelect => "select-ellipse",
			CanvasHost.CanvasTool.LassoSelect => "select-lasso",
			CanvasHost.CanvasTool.MoveSelectedPixels => "move",
			CanvasHost.CanvasTool.MoveSelection => "move-selection",
			CanvasHost.CanvasTool.Brush => "paintbrush",
			CanvasHost.CanvasTool.Pencil => "pencil",
			CanvasHost.CanvasTool.Shapes => "rectangle",
			CanvasHost.CanvasTool.Gradient => "gradient",
			CanvasHost.CanvasTool.Fill => "paintbucket",
			CanvasHost.CanvasTool.MagicWand => "select-magicwand",
			CanvasHost.CanvasTool.ColorPicker => "colorpicker",
			CanvasHost.CanvasTool.Recolor => "recolor",
			CanvasHost.CanvasTool.Zoom => "zoom",
			CanvasHost.CanvasTool.Pan => "pan",
			CanvasHost.CanvasTool.Eraser => "eraser",
			CanvasHost.CanvasTool.CloneStamp => "clonestamp",
			CanvasHost.CanvasTool.Text => "text",
			_ => null
		};
		if (name is null) return fallback;
		if (cache.TryGetValue (name, out string? cached)) return cached;
		using var stream = typeof (ToolIcons).Assembly.GetManifestResourceStream ($"tool-{name}-symbolic.svg")
			?? throw new InvalidOperationException ($"Missing tool icon: {name}");
		XElement svg = XElement.Load (stream);
		// These existing symbolic assets contain filled paths and polygons only.
		string paths = string.Join (" ", svg.Descendants ().Where (e => e.Name.LocalName is "path" or "polygon").Select (e =>
			e.Name.LocalName == "path" ? (string?) e.Attribute ("d") : "M " + (string?) e.Attribute ("points") + " Z"));
		cache.Add (name, paths);
		return paths;
	}
}
