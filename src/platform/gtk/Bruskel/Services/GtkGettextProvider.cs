using System;
using System.Globalization;
using Bruskel.Core;

namespace Bruskel;

/// <summary>
/// Translations through the native gettext used by GTK / libadwaita, so Bruskel's strings and the
/// toolkit's strings follow the same language.
/// </summary>
internal sealed class GtkGettextProvider : ITranslationProvider
{
	private const string BruskelTextDomain = "bruskel";
	private bool native_translations_available = true;

	public void Initialize (string localeDir, string languagePreference)
	{
		try {
			// This is called very early in startup, before GTK is initialized.
			GLib.Module.Initialize ();

			// A non-default language preference overrides any language setting from the environment.
			// Otherwise follow the dotnet UI culture in the absence of any env vars, since this
			// correctly picks up system language settings on macOS, for example.
			// The native gettext reads LANGUAGE, so set it to keep Bruskel and GTK consistent.
			string? langOverride = null;
			if (!string.IsNullOrEmpty (languagePreference)) {
				langOverride = languagePreference;
			} else if (GLib.Functions.Getenv ("LANGUAGE") is null) {
				// Convert names like en-CA to en_CA.
				langOverride = CultureInfo.CurrentUICulture.Name.Replace ('-', '_');
			}

			if (!string.IsNullOrEmpty (langOverride))
				GLib.Functions.Setenv ("LANGUAGE", langOverride, overwrite: true);

			IntlExtensions.BindTextDomain (BruskelTextDomain, localeDir);
			IntlExtensions.BindTextDomainCodeset (BruskelTextDomain, "UTF-8");
			IntlExtensions.TextDomain (BruskelTextDomain);
		} catch (DllNotFoundException) {
			native_translations_available = false;
		}
	}

	public string GetString (string text)
	{
		if (!native_translations_available)
			return text;

		try {
			return GLib.Functions.Dgettext (BruskelTextDomain, text);
		} catch (DllNotFoundException) {
			native_translations_available = false;
			return text;
		}
	}
}
