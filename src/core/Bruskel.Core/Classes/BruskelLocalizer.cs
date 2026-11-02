using Mono.Addins.Localization;

namespace Bruskel.Core;

/// <summary>Wrapper around Bruskel's translation template.</summary>
public sealed class BruskelLocalizer : IAddinLocalizer
{
	public string GetString (string msgid)
		=> Translations.GetString (msgid);
};
