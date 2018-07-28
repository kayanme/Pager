using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Benchmark.Paging.PhysicalLevel
{
    public class C:ManualConfig
    {
        public C()
        {
            Add(Job.Core.WithInvocationCount(96*2).WithLaunchCount(3).WithAnalyzeLaunchVariance(true));
            Add(DefaultConfig.Instance.GetExporters().ToArray());
            Add(DefaultConfig.Instance.GetLoggers().ToArray());
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

        }
    }
}