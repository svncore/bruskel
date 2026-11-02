using System.Threading.Tasks;
using Mono.Addins.Localization;

namespace Bruskel.Core;

public interface ILivePreviewSession
{
	void NotifyChanged ();
	Task<bool> LaunchSimpleEffectDialog (IAddinLocalizer? localizer = null);
}
