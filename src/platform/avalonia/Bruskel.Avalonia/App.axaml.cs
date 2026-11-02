using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Bruskel.Core;
using Bruskel.Application;

namespace Bruskel.Avalonia;

public sealed class App : global::Avalonia.Application
{
	public static BruskelApplication Services { get; } = BruskelApplication.Build (new NeutralImageFormatProvider (), includeActions: false);
	private const string ThemeSettingKey = "Bruskel.Theme";
	private static ISettingsService? themeSettings;

	public static void InitializeTheme (ISettingsService settings)
	{
		themeSettings = settings;
		string savedTheme = settings.GetSetting (ThemeSettingKey, "System");
		if (savedTheme.Equals ("Dark", System.StringComparison.OrdinalIgnoreCase))
			SetTheme (ThemeVariant.Dark);
		else if (savedTheme.Equals ("Light", System.StringComparison.OrdinalIgnoreCase))
			SetTheme (ThemeVariant.Light);
	}

	public static void SetTheme (ThemeVariant variant)
	{
		if (Current is not null)
			Current.RequestedThemeVariant = variant;
		themeSettings?.PutSetting (ThemeSettingKey, variant == ThemeVariant.Dark ? "Dark" : variant == ThemeVariant.Light ? "Light" : "System");
	}

	public static void ToggleTheme ()
	{
		if (Current is null)
			return;
		SetTheme (Current.ActualThemeVariant == ThemeVariant.Dark
			? ThemeVariant.Light
			: ThemeVariant.Dark);
	}
	public override void Initialize ()
	{
		AvaloniaXamlLoader.Load (this);
	}

	public override void OnFrameworkInitializationCompleted ()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			 desktop.MainWindow = new MainWindow (Services);

		base.OnFrameworkInitializationCompleted ();
	}
}
