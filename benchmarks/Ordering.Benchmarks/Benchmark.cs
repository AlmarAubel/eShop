using BenchmarkDotNet.Attributes;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;
using CardType = eShop.Ordering.API.Application.Queries.CardType;

namespace Ordering.Benchmarks;


public class Benchmark : BenchmarkBase
{
    [Benchmark]
    public async Task<Order> RawSql_GetOrderAsync()
    {
        return await OrderRawSqlQueries.GetOrderAsync( NumberGenerator.Next());
    }
    
    [Benchmark]
    public async Task<Order> EfCore_GetOrderAsync()
    {
        return await OrderQueries.GetOrderAsync( NumberGenerator.Next());
    }   
    
    [Benchmark]
    public async Task<List<OrderSummary>> RawSql_GetOrdersAsync()
    {
        var result =  await OrderRawSqlQueries.GetOrdersFromUserAsync(GetbuyerId());
        return result.ToList();
    }
    
    [Benchmark]
    public async Task<List<OrderSummary>> EfCore_GetOrdersAsync()
    {
        var result = await OrderQueries.GetOrdersFromUserAsync(GetbuyerId());
        return result.ToList();
    }
    
    [Benchmark]
    public async Task<List<CardType>> RawSql_GetCardTypesAsync()
    {  
        var result = await OrderRawSqlQueries.GetCardTypesAsync();
        return result.ToList();
    }   
    
    [Benchmark]
    public async Task<List<CardType>> EfCore_GetCardTypesAsync()
    {  
        var result = await OrderQueries.GetCardTypesAsync();
        return result.ToList();
    }
    
    private static readonly Func<OrderingContext, string, IAsyncEnumerable<OrderSummary>> OptimizedCompiled = EF.CompileAsyncQuery(
        (OrderingContext context, string buyerId) => context.Orders
            .Join(context.Buyers,
                order => order.BuyerId,
                buyer => buyer.Id,
                (order, buyer) => new { Order = order, Buyer = buyer })
            .Where(ob => ob.Buyer.IdentityGuid == buyerId)
            .OrderBy(o => o.Order.Id)
            .Select(ob => new OrderSummary
            {
                ordernumber = ob.Order.Id,
                date = ob.Order.OrderDate,
                status = ob.Order.OrderStatus.Name,
                total = (double)ob.Order.OrderItems.Sum(oi => oi.UnitPrice * oi.Units)
            })
    );
    
    [Benchmark]
    public async Task<List<OrderSummary>> Optimized_Compiled()
    { 
        var result = new List<OrderSummary>();
        await foreach (var orderSummary in OptimizedCompiled(OrderingContext, GetbuyerId()))
        {
            result.Add(orderSummary);
        }

        return  result;
    }
}

public class SequentialIntGenerator(int steps = 1)
{
    private int _currentValue;

    public int Next()
    {
        _currentValue += steps;
        return _currentValue > 1000 ? (_currentValue = steps) : _currentValue;
    }
}
