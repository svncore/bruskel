using Avalonia.Controls;
using Avalonia.Layout;
using Bruskel.Avalonia.Input;

namespace Bruskel.Avalonia;

public sealed class KeyboardSettingsWindow : Window
{
	public KeyboardSettingsWindow (BruskelKeyboardSettings settings)
	{
		Title = "Keyboard shortcuts";
		Width = 420;
		Height = 520;
		CanResize = false;

		var rows = new StackPanel { Spacing = 8 };
		rows.Children.Add (CreateRow ("Undo", BruskelKeyCommands.Undo, settings));
		rows.Children.Add (CreateRow ("Redo", BruskelKeyCommands.Redo, settings));
		rows.Children.Add (CreateRow ("Decrease brush", BruskelKeyCommands.DecreaseBrush, settings));
		rows.Children.Add (CreateRow ("Increase brush", BruskelKeyCommands.IncreaseBrush, settings));
		rows.Children.Add (CreateRow ("Toggle eraser", BruskelKeyCommands.ToggleEraser, settings));
		rows.Children.Add (CreateRow ("Brush", BruskelKeyCommands.Brush, settings));
		rows.Children.Add (CreateRow ("Pencil", BruskelKeyCommands.Pencil, settings));
		rows.Children.Add (CreateRow ("Shapes", BruskelKeyCommands.Shapes, settings));
		rows.Children.Add (CreateRow ("Line shape", BruskelKeyCommands.Line, settings));
		rows.Children.Add (CreateRow ("Rectangle shape", BruskelKeyCommands.Rectangle, settings));
		rows.Children.Add (CreateRow ("Fill", BruskelKeyCommands.Fill, settings));
		rows.Children.Add (CreateRow ("Color picker", BruskelKeyCommands.ColorPicker, settings));
		rows.Children.Add (CreateRow ("Zoom", BruskelKeyCommands.Zoom, settings));
		rows.Children.Add (CreateRow ("Fit canvas", BruskelKeyCommands.FitCanvas, settings));
		rows.Children.Add (CreateRow ("Actual size", BruskelKeyCommands.ActualSize, settings));
		rows.Children.Add (CreateRow ("Clear layer", BruskelKeyCommands.ClearLayer, settings));
		rows.Children.Add (CreateRow ("Copy selection", BruskelKeyCommands.Copy, settings));
		rows.Children.Add (CreateRow ("Paste selection", BruskelKeyCommands.Paste, settings));
		rows.Children.Add (CreateRow ("Select all", BruskelKeyCommands.SelectAll, settings));
		rows.Children.Add (CreateRow ("Deselect", BruskelKeyCommands.Deselect, settings));
		rows.Children.Add (CreateRow ("Duplicate layer", BruskelKeyCommands.DuplicateLayer, settings));
		rows.Children.Add (CreateRow ("Clear history", BruskelKeyCommands.ClearHistory, settings));
		var reset = new Button { Content = "Reset to defaults", HorizontalAlignment = HorizontalAlignment.Left };
		reset.Click += (_, _) => {
			settings.ResetAll ();
			Close ();
		};
		rows.Children.Add (reset);
		Content = new Border {
			Padding = new global::Avalonia.Thickness (20),
			Child = new ScrollViewer { Content = rows }
		};
	}

	private static Control CreateRow (string title, BruskelKeyCommand command, BruskelKeyboardSettings settings)
	{
		var input = new TextBox { Text = settings.GestureTextFor (command), Width = 150 };
		input.LostFocus += (_, _) => {
			if (!settings.TrySetGesture (command, input.Text))
				input.Text = settings.GestureTextFor (command);
		};
		var row = new Grid {
			ColumnDefinitions = new ColumnDefinitions ("*,Auto"),
			Children = {
				new TextBlock { Text = title, VerticalAlignment = VerticalAlignment.Center },
				input
			}
		};
		Grid.SetColumn (input, 1);
		return row;
	}
}
