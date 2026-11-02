using Bruskel.Core;

namespace Bruskel.Gui.Widgets;

public static class CommandMenuItemExtensions
{
	public static Gio.MenuItem ToGioMenuItem (this CommandMenuItem item)
		=> Gio.MenuItem.New (item.Label, item.ActionId);
}
