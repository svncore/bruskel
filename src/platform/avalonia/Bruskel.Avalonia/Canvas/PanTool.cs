using Bruskel.Core;

namespace Bruskel.Avalonia.Canvas;

/// <summary>Dedicated Paint.NET-style hand tool. CanvasHost owns the viewport drag.</summary>
internal sealed class PanTool : ICanvasTool
{
	public CanvasHost.CanvasTool Kind => CanvasHost.CanvasTool.Pan;
}
