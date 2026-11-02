using System.Collections.Generic;
using System.IO;
using Cairo;

namespace Bruskel.Core;

/// <summary>Platform-neutral palette loading contract for stream-based frontends.</summary>
public interface IPaletteStreamLoader
{
	List<Color> Load (Stream stream);
}
