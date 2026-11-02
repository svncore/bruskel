using System;
using System.IO;
using Avalonia;
using Bruskel.Core;

namespace Bruskel.Avalonia;

internal static class Program
{
	[STAThread]
	public static void Main (string[] args)
	{
		Translations.SetProvider (new ManagedGettextProvider ());
		Translations.Init (Path.Combine (AppContext.BaseDirectory, "locale"), string.Empty);
		BuildAvaloniaApp ().StartWithClassicDesktopLifetime (args);
	}

	public static AppBuilder BuildAvaloniaApp ()
		=> AppBuilder.Configure<App> ()
			.UsePlatformDetect ()
			.LogToTrace ();
}
