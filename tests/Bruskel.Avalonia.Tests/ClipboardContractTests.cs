using System.Threading.Tasks;
using NUnit.Framework;
using Bruskel.Core;

namespace Bruskel.Avalonia.Tests;

[TestFixture]
internal sealed class ClipboardContractTests
{
	[Test]
	public async Task ClipboardContractSupportsTextRoundTrip ()
	{
		var clipboard = new FakeClipboardService ();
		clipboard.SetText ("Bruskel");

		Assert.That (await clipboard.ReadTextAsync (), Is.EqualTo ("Bruskel"));
	}

	private sealed class FakeClipboardService : IClipboardService
	{
		private string? text;
		public Task<bool> ContainsImageAsync () => Task.FromResult (false);
		public Task<ClipboardImageData?> ReadImageAsync () => Task.FromResult<ClipboardImageData?> (null);
		public void WriteImage (ClipboardImageData image) { }
		public void SetText (string value) => text = value;
		public Task<string?> ReadTextAsync () => Task.FromResult (text);
	}
}
