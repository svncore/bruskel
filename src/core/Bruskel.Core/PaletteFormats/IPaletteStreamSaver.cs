using System.Collections.Generic;
using System.IO;
using Cairo;

namespace Bruskel.Core;

/// <summary>Platform-neutral palette saving contract for stream-based frontends.</summary>
public interface IPaletteStreamSaver
{
	void Save (IReadOnlyList<Color> colors, Stream stream);
}
