using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;

namespace Ordering.Benchmarks;

public class Config : ManualConfig
{
    public Config()
    {
        AddLogger(ConsoleLogger.Default);
        AddExporter(MarkdownExporter.GitHub);
        AddExporter(HtmlExporter.Default);

        var md = MemoryDiagnoser.Default;
        AddDiagnoser(md);
        AddColumn(TargetMethodColumn.Method);
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.StdDev);
        AddColumn(StatisticColumn.Error);
        AddColumn(BaselineRatioColumn.RatioMean);
        AddColumnProvider(DefaultColumnProviders.Metrics);

        AddJob(Job.ShortRun
            .WithLaunchCount(1)
            .WithWarmupCount(2)
            //.WithUnrollFactor(Iterations)
            .WithIterationCount(10)
        );
        Orderer = new DefaultOrderer(SummaryOrderPolicy.Declared);
        Options |= ConfigOptions.JoinSummary;
    }
}
