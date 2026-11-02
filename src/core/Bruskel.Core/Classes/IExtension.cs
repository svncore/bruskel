using Bruskel.Core;

[assembly: Mono.Addins.AddinRoot ("Bruskel", BruskelCore.ApplicationVersion, CompatVersion = BruskelCore.AddinCompatVersion)]

namespace Bruskel.Core;

[Mono.Addins.TypeExtensionPoint]
public interface IExtension
{
	void Initialize ();
	void Uninitialize ();
}
