# Pencil


## Behavior

Edits the active layer pixel by pixel without soft antialiasing. Left uses Primary and right uses Secondary. Each drag is committed as a history operation while retaining hard pixel edges.

## Shortcut and appearance

`P` activates Pencil. The button and cursor show a pencil; the toolbar exposes size and color but never turns the stroke into a soft brush.

## Bruskel implementation

`PencilTool.cs` uses Core pixel operations. Avalonia's `BrushTool` has a separate `DrawPencilLine` path, preserving hard-edged rendering.

## Improvements

Test 100% and high zoom, half-pixel coordinates, transparency, and guarantee that Pencil does not inherit brush hardness or antialiasing.

