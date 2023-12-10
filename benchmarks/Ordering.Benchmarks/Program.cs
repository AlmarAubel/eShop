
using BenchmarkDotNet.Running;
using Ordering.Benchmarks;

var debug = false;

#if DEBUG
debug = true;
#endif

if (!debug)
{
    BenchmarkRunner.Run<Benchmark>(new Config());
    //BenchmarkRunner.Run<BenchmarkGetOrders>(new Config());
}

