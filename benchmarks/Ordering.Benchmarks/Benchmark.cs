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
using CardType = eShop.Ordering.API.Application.Queries.CardType;

namespace Ordering.Benchmarks;
[MinIterationCount(1)]
[MaxIterationCount(10)]
[WarmupCount(3)]

[MemoryDiagnoser(false)]
public class Benchmark 
{
    private DbConnection _connection = default!;
    private OrderRawSqlQueries _orderRawSqlQueries = default!;
    private OrderQueries _orderQueries = default!;
    private OrderingContext _orderingContext = default!;

    private string _buyerId = default!;
    //private int _orderId = 500;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var dbConnectionFactory = new DbConnectionFactory();
        _connection = await dbConnectionFactory.CreateConnectionAsync();

        var optionsBuilder = new DbContextOptionsBuilder<OrderingContext>();
        optionsBuilder.UseNpgsql(_connection);

        _orderingContext = new OrderingContext(optionsBuilder.Options);
        _orderRawSqlQueries = new OrderRawSqlQueries(NpgsqlDataSource.Create(dbConnectionFactory.ConnectionString));
        
        var seeder = new Seeder(_orderingContext);
        _buyerId = await seeder.Seed();
        await _orderingContext.Database.EnsureCreatedAsync();
        
        _orderingContext.ChangeTracker.Clear();
    }
    
    // [Benchmark]
    // public async Task<Order> RawSql_GetOrderAsync()
    // {
    //     return await _orderRawSqlQueries.GetOrderAsync(_orderId);
    // }
    //
    // [Benchmark]
    // public async Task<Order> EfCore_GetOrderAsync()
    // {
    //     return await _orderQueries.GetOrderAsync(_orderId);
    // }
    
    [Benchmark]
    public async Task<List<OrderSummary>> RawSql_GetOrdersAsync()
    {
        var result =  await _orderRawSqlQueries.GetOrdersFromUserAsync(_buyerId);
        return result.ToList();
    }
    
    [Benchmark]
    public async Task<List<OrderSummary>> EfCore_GetOrdersAsync()
    {
        var result = await _orderQueries.GetOrdersFromUserAsync(_buyerId);
        return result.ToList();
    }
    
    //
    // [Benchmark]
    // public async Task<List<CardType>> RawSql_GetCardTypesAsync()
    // {  
    //     var result = await _orderRawSqlQueries.GetCardTypesAsync();
    //     return result.ToList();
    // }   
    //
    // [Benchmark]
    // public async Task<List<CardType>> EfCore_GetCardTypesAsync()
    // {  
    //     var result = await _orderQueries.GetCardTypesAsync();
    //     return result.ToList();
    // }
    
    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _connection.Dispose();
    }
}
