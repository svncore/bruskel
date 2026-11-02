using NUnit.Framework;
using System;
using System.Collections.Generic;
using Bruskel.Core;
using Bruskel.Avalonia.Input;

namespace Bruskel.Avalonia.Tests;

[TestFixture]
internal sealed class BruskelKeyboardSettingsTests
{
	[Test]
	public void DefaultsAreStable ()
	{
		BruskelKeyboardSettings settings = new ();

		Assert.That (settings.GestureTextFor (BruskelKeyCommands.Undo), Is.EqualTo ("Ctrl+Z"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.DecreaseBrush), Is.EqualTo ("["));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.ToggleEraser), Is.EqualTo ("E"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.Text), Is.EqualTo ("T"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.ClearLayer), Is.EqualTo ("Delete"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.ActualSize), Is.EqualTo ("1"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.Copy), Is.EqualTo ("Ctrl+C"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.CopyImage), Is.EqualTo ("Ctrl+Alt+C"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.CopySelectionImage), Is.EqualTo ("Ctrl+Shift+C"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.PasteImageAsLayer), Is.EqualTo ("Ctrl+Shift+V"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.PasteImageAnnotation), Is.EqualTo ("Ctrl+Alt+V"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.SelectAll), Is.EqualTo ("Ctrl+A"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.DuplicateLayer), Is.EqualTo ("Ctrl+Shift+D"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.DuplicateAnnotation), Is.EqualTo ("Ctrl+D"));
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.ClearHistory), Is.EqualTo ("Ctrl+Shift+Delete"));
		settings.SetGesture (BruskelKeyCommands.Undo, "Ctrl+U");
		settings.ResetAll ();
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.Undo), Is.EqualTo ("Ctrl+Z"));
		Assert.That (settings.TrySetGesture (BruskelKeyCommands.Redo, "Ctrl+Z"), Is.False);
		Assert.That (settings.GestureTextFor (BruskelKeyCommands.Redo), Is.EqualTo ("Ctrl+Y"));
	}

	[Test]
	public void UserBindingReplacesDefault ()
	{
		BruskelKeyboardSettings settings = new ();

		settings.SetGesture (BruskelKeyCommands.Undo, "Ctrl+U");

		Assert.That (settings.GestureTextFor (BruskelKeyCommands.Undo), Is.EqualTo ("Ctrl+U"));
		Assert.That (settings.GestureFor (BruskelKeyCommands.Undo), Is.Not.Null);
	}

	[Test]
	public void UserBindingIsLoadedFromSettings ()
	{
		var stored = new MemorySettings ();
		BruskelKeyboardSettings first = new (stored);
		Assert.That (first.TrySetGesture (BruskelKeyCommands.Undo, "Ctrl+U"), Is.True);

		BruskelKeyboardSettings reloaded = new (stored);

		Assert.That (reloaded.GestureTextFor (BruskelKeyCommands.Undo), Is.EqualTo ("Ctrl+U"));
	}

	private sealed class MemorySettings : ISettingsService
	{
		private readonly Dictionary<string, object> values = new ();

		#pragma warning disable CS0067
		public event EventHandler? SaveSettingsBeforeQuit;
		public event EventHandler<SettingChangedEventArgs>? SettingChanged;
		#pragma warning restore CS0067

		public T GetSetting<T> (string key, T defaultValue)
			=> values.TryGetValue (key, out object? value) && value is T typed ? typed : defaultValue;

		public string GetUserSettingsDirectory () => string.Empty;

		public void PutSetting (string key, object value) => values[key] = value;
	}
}
