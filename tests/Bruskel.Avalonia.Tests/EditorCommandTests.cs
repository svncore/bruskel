using NUnit.Framework;
using Bruskel.Avalonia.Editor.Commands;
using Bruskel.Avalonia.Input;

namespace Bruskel.Avalonia.Tests;

[TestFixture]
internal sealed class EditorCommandTests
{
	[Test]
	public void RegistryExecutesRegisteredCommand ()
	{
		var registry = new EditorCommandRegistry ();
		bool executed = false;
		registry.Register (new EditorCommand ("edit.undo", "Undo", () => executed = true));

		Assert.That (registry.TryExecute ("edit.undo"), Is.True);
		Assert.That (executed, Is.True);
	}

	[Test]
	public void DisabledCommandIsNotExecuted ()
	{
		var registry = new EditorCommandRegistry ();
		registry.Register (new EditorCommand ("edit.undo", "Undo", () => Assert.Fail (), () => false));

		Assert.That (registry.TryExecute ("edit.undo"), Is.False);
	}

	[Test]
	public void KeyCommandUsesTheSameStableId ()
	{
		var registry = new EditorCommandRegistry ();
		registry.Register (BruskelKeyCommands.Undo, () => { });

		Assert.That (registry.Contains ("edit.undo"), Is.True);
	}
}
