using Pinta.Core;

namespace Pintora.Avalonia.Canvas;

/// <summary>Dedicated Paint.NET-style hand tool. CanvasHost owns the viewport drag.</summary>
internal sealed class PanTool : ICanvasTool
{
	public CanvasHost.CanvasTool Kind => CanvasHost.CanvasTool.Pan;
}
