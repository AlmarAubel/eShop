using System.Data.Common;
using BenchmarkDotNet.Attributes;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Queries.eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Infrastructure;
using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ordering.Benchmarks.Database;

namespace Ordering.Benchmarks;

[MaxIterationCount(20)]
[MemoryDiagnoser(false)]
public class Benchmark 
{
    private DbConnection _connection = default!;
    private OrderRawSqlQueries _orderRawSqlQueries = default!;
    private OrderQueries _orderQueries = default!;
    private List<eShop.Ordering.Domain.AggregatesModel.OrderAggregate.Order> _orders = default!;
    private OrderingContext _orderingContext;
    private List<Buyer> _buyers;
    private string _buyerId;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var dbConnectionFactory = new DbConnectionFactory();
        _connection = await dbConnectionFactory.CreateConnectionAsync();

        var optionsBuilder = new DbContextOptionsBuilder<OrderingContext>();
        optionsBuilder.UseNpgsql(_connection);
        optionsBuilder.EnableSensitiveDataLogging();

        _orderingContext = new OrderingContext(optionsBuilder.Options);
        _orderRawSqlQueries = new OrderRawSqlQueries(NpgsqlDataSource.Create(dbConnectionFactory.ConnectionString));
        _orderQueries = new OrderQueries(_orderingContext);
        await _orderingContext.Database.EnsureCreatedAsync();
        await Seed(_orderingContext);

        _orderingContext.ChangeTracker.Clear();
    }
    
    [IterationSetup]
    public void IterationSetup() => _orderingContext.ChangeTracker.Clear();
    
    [Benchmark]
    [ArgumentsSource(nameof(OrderIds))]
    public async Task<Order> RawSql_GetOrderAsync(int orderId)
    {
        return await _orderRawSqlQueries.GetOrderAsync(orderId);
    }

    [Benchmark]
    [ArgumentsSource(nameof(OrderIds))]
    public async Task<Order> EfCore_GetOrderAsync(int orderId)
    {
        return await _orderQueries.GetOrderAsync(orderId);
    }
    
    [Benchmark]
    public async Task<IEnumerable<OrderSummary>> RawSql_GetOrdersAsync()
    {
        return await _orderRawSqlQueries.GetOrdersFromUserAsync(_buyerId);
    }
    
    [Benchmark]
    public async Task<IEnumerable<OrderSummary>> EfCore_GetOrdersAsync()
    {
        return await _orderQueries.GetOrdersFromUserAsync(_buyerId);
    }
    
    public IEnumerable<object> OrderIds()
    {
        // yield return 1;
        // yield return 50;
        // yield return 100;
        // yield return 250;
        yield return 500;
        // yield return 750;
        // yield return 1000;
    }
    
    private async Task Seed(OrderingContext orderingContext1)
    {
        var orderingContextSeed = new OrderingContextSeed();

        await orderingContextSeed.SeedAsync(orderingContext1);
        var orderGenerator = new OrderGenerator(orderingContext1, new Random(42));
        _buyers = await orderGenerator.GenerateBuyers(100);
        _orders = await orderGenerator.GenerateOrders(1000,_buyers);
        _buyerId = _buyers.Skip(50).First().IdentityGuid;
    }
    

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _connection.Dispose();
    }
}
