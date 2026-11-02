using System;
using Bruskel.Core;

namespace Bruskel.Effects;

public class MockSystemService : ISystemService
{
	public int RenderThreads { get; set; } = Environment.ProcessorCount;
	public OS OperatingSystem { get; } = OS.Other;
	public string AltLabel => "Alt";
	public string CtrlLabel => "Ctrl";
}
