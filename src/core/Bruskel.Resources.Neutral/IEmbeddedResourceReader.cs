using System.Reflection;

namespace Bruskel.Resources;

public interface IEmbeddedResourceReader
{
	byte[]? Read (Assembly assembly, string name);
}
