
using System.Diagnostics;
using BenchmarkDotNet.Running;
using Ordering.Benchmarks;

BenchmarkRunner.Run<Benchmark>();
// //
// //
// var getOrdersBenchmark = new Benchmark();
// await getOrdersBenchmark.GlobalSetup();
//
//
// await RunCommand(getOrdersBenchmark);
// await RunCommand(getOrdersBenchmark);
//
//
//
//
// async Task RunCommand(Benchmark benchmark)
// {
//     var stopwatch = Stopwatch.StartNew();
//
//     var order = await benchmark.EfCore_GetOrderAsync();
//     var order2 = await benchmark.EfCore_NaiveGetOrderAsync();
//
//     stopwatch.Stop();
//     Console.WriteLine($"Execution Time: {stopwatch.ElapsedTicks} ms order number: {order.total} count{order.ordernumber}");
// }
