using System.Data.Common;
using BenchmarkDotNet.Attributes;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Queries.eShop.Ordering.API.Application.Queries;
using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ordering.Benchmarks.Database;
using CardType = eShop.Ordering.API.Application.Queries.CardType;

namespace Ordering.Benchmarks;

[MinIterationCount(1)]
[MaxIterationCount(20)]
[WarmupCount(10)]
[MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser(false)]
public class GetOrdersBenchmark
{
    private DbConnection _connection = default!;
    private OrderRawSqlQueries _orderRawSqlQueries = default!;
    private OrderingContext _orderingContext = default!;
    private string _buyerId = default!;
    
    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var dbConnectionFactory = new DbConnectionFactory();
        _connection = await dbConnectionFactory.CreateConnectionAsync();

        var optionsBuilder = new DbContextOptionsBuilder<OrderingContext>();
        optionsBuilder.UseNpgsql(_connection);
        
        var loggerFactory = LoggerFactory.Create(builder => 
        {
            builder
                .AddFilter((category, level) =>
                    category == DbLoggerCategory.Database.Command.Name
                    && level == LogLevel.Information)
                .AddConsole();
        });
        
        _orderingContext = new OrderingContext(optionsBuilder.Options);
        _orderRawSqlQueries = new OrderRawSqlQueries(NpgsqlDataSource.Create(dbConnectionFactory.ConnectionString));
        await _orderingContext.Database.EnsureCreatedAsync();

        var seeder = new Seeder(_orderingContext);
        _buyerId = await seeder.Seed();

        _orderingContext.ChangeTracker.Clear();
        # if DEBUG
        optionsBuilder.UseLoggerFactory(loggerFactory);
        #endif
        _orderingContext = new OrderingContext(optionsBuilder.Options);
    }

    [Benchmark(Baseline = true)]
    public async Task<List<OrderSummary>> RawSql()
    {
        var result = await _orderRawSqlQueries.GetOrdersFromUserAsync(_buyerId);
        return result.ToList();
    }

    [Benchmark]
    public async Task<List<OrderSummary>> Naive()
    {
        return await _orderingContext.Orders
            .Include(o => o.OrderItems)
            .Join(_orderingContext.Buyers,
                o => o.BuyerId,
                b => b.Id,
                (order, buyer) => new { order, buyer })
            .Where(ob => ob.buyer.IdentityGuid == _buyerId)
            .Select(ob => new OrderSummary
            {
                ordernumber = ob.order.Id, 
                date = ob.order.GetOrderDate(), 
                status = ob.order.OrderStatus.Name, 
                total = (double)ob.order.GetTotal()
            })
            .ToListAsync();
    }

    [Benchmark]
    public async Task<List<OrderSummary>> NaiveAsNoTracking()
    {
        return await _orderingContext.Orders
            .Include(o => o.OrderItems)
            .Join(_orderingContext.Buyers,
                o => o.BuyerId,
                b => b.Id,
                (order, buyer) => new { order, buyer })
            .Where(ob => ob.buyer.IdentityGuid == _buyerId)
            .Select(ob => new OrderSummary
            {
                ordernumber = ob.order.Id, 
                date = ob.order.GetOrderDate(), 
                status = ob.order.OrderStatus.Name, 
                total = (double)ob.order.GetTotal()
            })
            .AsNoTracking()
            .ToListAsync();
    }

    [Benchmark]
    public async Task<List<OrderSummary>> Optimized()
    {
        //_orderingContext.ChangeTracker.Clear();
        return await _orderingContext.Orders
            .Join(_orderingContext.Buyers,
                o => o.BuyerId,
                b => b.Id,
                (order, buyer) => new { order, buyer })
            .Where(ob => ob.buyer.IdentityGuid == _buyerId)
            .Select(ob => new OrderSummary
            {
                ordernumber = ob.order.Id,
                date = ob.order.OrderDate,
                status = ob.order.OrderStatus.Name,
                total = (double)ob.order.OrderItems.Sum(oi => oi.UnitPrice * oi.Units)
            })
            .ToListAsync();
    }

    [Benchmark]
    public async Task<List<OrderSummary>> OptimizedAsNoTracking()
    {
        return await _orderingContext.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatus)
            .Join(_orderingContext.Buyers,
                o => o.BuyerId,
                b => b.Id,
                (order, buyer) => new { order, buyer })
            .Where(ob => ob.buyer.IdentityGuid == _buyerId)
            .Select(ob => new OrderSummary
            {
                ordernumber = ob.order.Id,
                date = ob.order.OrderDate,
                status = ob.order.OrderStatus.Name,
                total = (double)ob.order.OrderItems.Sum(oi => oi.UnitPrice * oi.Units)
            })
            .AsNoTracking()
            .ToListAsync();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _connection.Dispose();
    }
}
