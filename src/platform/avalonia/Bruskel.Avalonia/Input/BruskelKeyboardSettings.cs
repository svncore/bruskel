using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Bruskel.Core;

namespace Bruskel.Avalonia.Input;

public sealed record BruskelKeyCommand (string Id, string DefaultGesture);

public static class BruskelKeyCommands
{
	public static readonly BruskelKeyCommand Undo = new ("edit.undo", "Ctrl+Z");
	public static readonly BruskelKeyCommand Redo = new ("edit.redo", "Ctrl+Y");
	public static readonly BruskelKeyCommand DecreaseBrush = new ("brush.decrease", "[");
	public static readonly BruskelKeyCommand IncreaseBrush = new ("brush.increase", "]");
	public static readonly BruskelKeyCommand ToggleEraser = new ("tool.toggle-eraser", "E");
	public static readonly BruskelKeyCommand Brush = new ("tool.brush", "B");
	public static readonly BruskelKeyCommand Pencil = new ("tool.pencil", "P");
	public static readonly BruskelKeyCommand Text = new ("tool.text", "T");
	public static readonly BruskelKeyCommand Shapes = new ("tool.shapes", "O");
	public static readonly BruskelKeyCommand Line = new ("tool.line", "L");
	public static readonly BruskelKeyCommand Rectangle = new ("tool.rectangle", "R");
	public static readonly BruskelKeyCommand Fill = new ("tool.fill", "F");
	public static readonly BruskelKeyCommand ColorPicker = new ("tool.color-picker", "I");
	public static readonly BruskelKeyCommand Zoom = new ("tool.zoom", "Z");
	public static readonly BruskelKeyCommand FitCanvas = new ("view.fit-canvas", "Home");
	public static readonly BruskelKeyCommand ActualSize = new ("view.actual-size", "1");
	public static readonly BruskelKeyCommand ClearLayer = new ("edit.clear-layer", "Delete");
	public static readonly BruskelKeyCommand SwapColors = new ("colors.swap", "X");
	public static readonly BruskelKeyCommand Copy = new ("edit.copy", "Ctrl+C");
	public static readonly BruskelKeyCommand CopyImage = new ("edit.copy-image", "Ctrl+Alt+C");
	public static readonly BruskelKeyCommand CopySelectionImage = new ("edit.copy-selection-image", "Ctrl+Shift+C");
	public static readonly BruskelKeyCommand Paste = new ("edit.paste", "Ctrl+V");
	public static readonly BruskelKeyCommand PasteImageAsLayer = new ("edit.paste-image-layer", "Ctrl+Shift+V");
	public static readonly BruskelKeyCommand PasteImageAnnotation = new ("edit.paste-image-annotation", "Ctrl+Alt+V");
	public static readonly BruskelKeyCommand SelectAll = new ("edit.select-all", "Ctrl+A");
	public static readonly BruskelKeyCommand Deselect = new ("edit.deselect", "Ctrl+Shift+A");
	public static readonly BruskelKeyCommand DuplicateLayer = new ("layer.duplicate", "Ctrl+Shift+D");
	public static readonly BruskelKeyCommand DuplicateAnnotation = new ("annotation.duplicate", "Ctrl+D");
	public static readonly BruskelKeyCommand ClearHistory = new ("edit.clear-history", "Ctrl+Shift+Delete");
	public static readonly BruskelKeyCommand Save = new ("file.save", "Ctrl+S");
	public static readonly BruskelKeyCommand SaveAll = new ("file.save-all", "Ctrl+Shift+S");
	public static readonly BruskelKeyCommand CloseDocument = new ("file.close", "Ctrl+W");
	public static readonly BruskelKeyCommand Exit = new ("app.exit", "Ctrl+Q");

	public static IReadOnlyList<BruskelKeyCommand> All { get; } = [Undo, Redo, DecreaseBrush, IncreaseBrush, ToggleEraser, Brush, Pencil, Text, Shapes, Line, Rectangle, Fill, ColorPicker, Zoom, FitCanvas, ActualSize, ClearLayer, SwapColors, Copy, CopyImage, CopySelectionImage, Paste, PasteImageAsLayer, PasteImageAnnotation, SelectAll, Deselect, DuplicateLayer, DuplicateAnnotation, ClearHistory, Save, SaveAll, CloseDocument, Exit];
}

public sealed class BruskelKeyboardSettings
{
	private readonly ISettingsService? settings;
	private readonly Dictionary<string, string> bindings = new (StringComparer.Ordinal);

	public BruskelKeyboardSettings (ISettingsService? settings = null)
	{
		this.settings = settings;
		foreach (BruskelKeyCommand command in BruskelKeyCommands.All) {
			string key = SettingKey (command);
			string value = settings?.GetSetting (key, command.DefaultGesture) ?? command.DefaultGesture;
			bindings[command.Id] = Normalize (value) ?? command.DefaultGesture;
		}
	}

	public string GestureTextFor (BruskelKeyCommand command)
		=> bindings.TryGetValue (command.Id, out string? value) ? value : command.DefaultGesture;

	public KeyGesture? GestureFor (BruskelKeyCommand command)
		=> Parse (GestureTextFor (command));

	public bool Matches (KeyEventArgs e, BruskelKeyCommand command)
		=> GestureFor (command)?.Matches (e) == true;

	public void SetGesture (BruskelKeyCommand command, string? gesture)
	{
		string value = Normalize (gesture) ?? string.Empty;
		if (settings is not null)
			settings.PutSetting (SettingKey (command), value);
		bindings[command.Id] = value;
	}

	public bool TrySetGesture (BruskelKeyCommand command, string? gesture)
	{
		string value = Normalize (gesture) ?? string.Empty;
		if (!string.IsNullOrEmpty (value) && bindings.Any (pair => pair.Key != command.Id && pair.Value == value))
			return false;
		SetGesture (command, value);
		return true;
	}

	public void ResetAll ()
	{
		foreach (BruskelKeyCommand command in BruskelKeyCommands.All)
			SetGesture (command, command.DefaultGesture);
	}

	private static string SettingKey (BruskelKeyCommand command) => $"Bruskel.Keyboard.{command.Id}";

	private static KeyGesture? Parse (string? text)
	{
		if (string.IsNullOrWhiteSpace (text)) return null;
		string[] parts = text.Split ('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length == 0) return null;
		KeyModifiers modifiers = KeyModifiers.None;
		string keyText = parts[^1];
		foreach (string part in parts[..^1]) {
			if (part.Equals ("Ctrl", StringComparison.OrdinalIgnoreCase)) modifiers |= KeyModifiers.Control;
			else if (part.Equals ("Shift", StringComparison.OrdinalIgnoreCase)) modifiers |= KeyModifiers.Shift;
			else if (part.Equals ("Alt", StringComparison.OrdinalIgnoreCase)) modifiers |= KeyModifiers.Alt;
			else return null;
		}
		if (keyText == "[") keyText = nameof (Key.OemOpenBrackets);
		if (keyText == "]") keyText = nameof (Key.OemCloseBrackets);
		if (keyText.Length == 1 && char.IsDigit (keyText[0])) keyText = $"D{keyText}";
		if (!Enum.TryParse (keyText, true, out Key key)) return null;
		return new KeyGesture (key, modifiers);
	}

	private static string? Normalize (string? text)
	{
		KeyGesture? gesture = Parse (text);
		if (gesture is null) return string.IsNullOrWhiteSpace (text) ? string.Empty : null;
		string modifiers = gesture.KeyModifiers.HasFlag (KeyModifiers.Control) ? "Ctrl+" : string.Empty;
		modifiers += gesture.KeyModifiers.HasFlag (KeyModifiers.Shift) ? "Shift+" : string.Empty;
		modifiers += gesture.KeyModifiers.HasFlag (KeyModifiers.Alt) ? "Alt+" : string.Empty;
		string key = gesture.Key switch {
			Key.OemOpenBrackets => "[",
			Key.OemCloseBrackets => "]",
			>= Key.D0 and <= Key.D9 => ((int) gesture.Key - (int) Key.D0).ToString (),
			_ => gesture.Key.ToString ()
		};
		return modifiers + key;
	}
}
