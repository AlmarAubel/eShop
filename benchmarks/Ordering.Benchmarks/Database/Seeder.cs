using eShop.Ordering.API.Infrastructure;
using eShop.Ordering.Infrastructure;

namespace Ordering.Benchmarks.Database;

public class Seeder
{
    private readonly OrderingContext _orderingContext;
    private readonly OrderingContextSeed _orderingContextSeed = new ();

    public Seeder(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }

    public async Task<List<string>> Seed()
    {
        await _orderingContextSeed.SeedAsync(_orderingContext);
        var orderGenerator = new OrderGenerator(_orderingContext, new Random(42));
        var buyers = await orderGenerator.GenerateBuyers(100);

        await orderGenerator.GenerateOrders(1000, buyers);
        return buyers.Select(b => b.IdentityGuid).ToList();
    }
}
