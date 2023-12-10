using BenchmarkDotNet.Attributes;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ordering.Benchmarks;

public class BenchmarkGetOrders: BenchmarkBase
{

    [Benchmark(Baseline = true)]
    public async Task<List<OrderSummary>> RawSql()
    {
        var result = await OrderRawSqlQueries.GetOrdersFromUserAsync(GetbuyerId());
        return result.ToList();
    }
    
    [Benchmark]
    public async Task<List<OrderSummary>> Naive()
    {
        return await OrderingContext.Orders
            .Include(o => o.OrderItems)
            .Join(OrderingContext.Buyers,
                o => o.BuyerId,
                b => b.Id,
                (order, buyer) => new { order, buyer })
            .Where(ob => ob.buyer.IdentityGuid == GetbuyerId())
            .OrderBy(o => o.order.Id)
            .Select(ob => new OrderSummary
            {
                ordernumber = ob.order.Id, 
                date = ob.order.GetOrderDate(), 
                status = ob.order.OrderStatus.Name, 
                total = (double)ob.order.GetTotal()
            })
            .ToListAsync();
    }

    // [Benchmark]
    // public async Task<List<OrderSummary>> NaiveAsNoTracking()
    // {
    //     return await _orderingContext.Orders
    //         .Include(o => o.OrderItems)
    //         .Join(_orderingContext.Buyers,
    //             o => o.BuyerId,
    //             b => b.Id,
    //             (order, buyer) => new { order, buyer })
    //         .Where(ob => ob.buyer.IdentityGuid == _buyerId)
    //         .Select(ob => new OrderSummary
    //         {
    //             ordernumber = ob.order.Id, 
    //             date = ob.order.GetOrderDate(), 
    //             status = ob.order.OrderStatus.Name, 
    //             total = (double)ob.order.GetTotal()
    //         })
    //         .AsNoTracking()
    //         .ToListAsync();
    // }

    [Benchmark]
    public async Task<List<OrderSummary>> Optimized()
    {
        //_orderingContext.ChangeTracker.Clear();
        return await OrderingContext.Orders
            .Join(OrderingContext.Buyers,
                o => o.BuyerId,
                b => b.Id,
                (order, buyer) => new { order, buyer })
            .Where(ob => ob.buyer.IdentityGuid == GetbuyerId())
            .OrderBy(o => o.order.Id)
            .Select(ob => new OrderSummary
            {
                ordernumber = ob.order.Id,
                date = ob.order.OrderDate,
                status = ob.order.OrderStatus.Name,
                total = (double)ob.order.OrderItems.Sum(oi => oi.UnitPrice * oi.Units)
            })
            .ToListAsync();
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

    [Benchmark]
    public async Task<List<OrderSummary>> OptimizedAsNoTracking()
    {
        return await OrderingContext.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatus)
            .Join(OrderingContext.Buyers,
                o => o.BuyerId,
                b => b.Id,
                (order, buyer) => new { order, buyer })
            .Where(ob => ob.buyer.IdentityGuid == GetbuyerId())
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
    
}
