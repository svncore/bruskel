using System.Collections.Generic;

namespace Bruskel.Core;

/// <summary>Platform-neutral palette format catalog.</summary>
public interface IPaletteFormatCatalog
{
	IReadOnlyList<IPaletteFormatDescriptor> Formats { get; }
}
