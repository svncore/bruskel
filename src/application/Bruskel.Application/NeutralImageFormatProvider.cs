using System.Collections.Generic;
using Bruskel.Core;

namespace Bruskel.Application;

/// <summary>Default provider for frontends that supply their own codecs.</summary>
public sealed class NeutralImageFormatProvider : IImageFormatProvider
{
	public IEnumerable<FormatDescriptor> GetFormats ()
	{
		yield break;
	}
}
