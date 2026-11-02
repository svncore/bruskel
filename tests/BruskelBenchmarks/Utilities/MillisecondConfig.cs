using BenchmarkDotNet.Configs;
using Perfolizer.Horology;

namespace BruskelBenchmarks;

internal class MillisecondConfig : ManualConfig
{
	public MillisecondConfig ()
	{
		SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default.WithTimeUnit (TimeUnit.Millisecond);
	}
}
