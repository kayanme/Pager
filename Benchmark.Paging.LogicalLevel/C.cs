using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Benchmark.Paging.LogicalLevel
{
    public class C : ManualConfig
    {
        public C()
        {
            Add(Job.Clr.WithInvocationCount(96).WithLaunchCount(3).WithAnalyzeLaunchVariance(true));
            Add(DefaultConfig.Instance.GetExporters().ToArray());
            Add(DefaultConfig.Instance.GetLoggers().ToArray());
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

        }
    }
}