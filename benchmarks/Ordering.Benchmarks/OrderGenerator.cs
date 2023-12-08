using System.Data;
using Bogus;
using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Infrastructure;

namespace Ordering.Benchmarks;

public class OrderGenerator
{
    private readonly OrderingContext _orderingContext;

    public OrderGenerator(OrderingContext orderingContext, Random random)
    {
        _orderingContext = orderingContext;
        Randomizer.Seed = random;
    }
   
    public async Task<List<Buyer>> GenerateBuyers(int count)
    {
        var buyers = Fakers.BuyerFaker.Generate(count);
        _orderingContext.AddRange(buyers);
        await _orderingContext.SaveChangesAsync();
        return buyers;
    }
    public async Task<List<Order>> GenerateOrders(int count, List<Buyer> buyers)
    {
        
        var orders = Fakers.OrderFaker(buyers).Generate(count);
        _orderingContext.AddRange(orders);
        await _orderingContext.SaveChangesAsync();
        return orders;
    }
}

public static class Fakers
{
    public static Faker<Order> OrderFaker(List<Buyer> buyers)=>
        new Faker<Order>()
            .CustomInstantiator(f =>
            {
                var buyer = f.PickRandom(buyers);
                var address = new Address( f.Address.StreetName(), f.Address.City(), f.Address.State(), f.Address.Country(), f.Address.ZipCode()); // Replace with appropriate parameters
                var order = new Order(
                    userId: f.Random.Guid().ToString(),
                    userName: f.Person.FullName,
                    address: address,
                    cardTypeId: f.Random.Int(1, 5),
                    cardNumber: f.Finance.CreditCardNumber(),
                    cardSecurityNumber: f.Finance.CreditCardCvv(),
                    cardHolderName: f.Person.FullName,
                    cardExpiration: f.Date.Future(),
                    buyerId: buyer.Id,
                    paymentMethodId: f.PickRandom( buyer.PaymentMethods.Select(x=>x.Id).ToList())
                );

                int itemsCount = f.Random.Int(1, 5);
                for (int i = 0; i < itemsCount; i++)
                {
                    order.AddOrderItem(
                        productId: f.Random.Int(1, 100),
                        productName: f.Commerce.ProductName(),
                        unitPrice: f.Random.Decimal(0.5m, 100m),
                        discount: f.Random.Decimal(0, 0.3m),
                        pictureUrl: f.Image.PicsumUrl(),
                        units: f.Random.Int(1, 10)
                    );
                }

                return order;
            });
    public static Faker<Buyer> BuyerFaker=>
        new Faker<Buyer>()
            .CustomInstantiator(f =>
            {
                var buyer = new Buyer(
                    identity: f.Random.Guid().ToString(), 
                    name: f.Person.FullName 
                );
                
                int paymentMethodsCount = f.Random.Int(1, 3); 
                for (int i = 0; i < paymentMethodsCount; i++)
                {
                    buyer.VerifyOrAddPaymentMethod(
                        cardTypeId: f.Random.Int(1, 3), 
                        alias: f.Lorem.Word(),
                        cardNumber: f.Finance.CreditCardNumber(),
                        securityNumber: f.Random.Number(100, 999).ToString(),
                        cardHolderName: f.Person.FullName,
                        expiration: f.Date.Future().ToUniversalTime(),
                        orderId: f.Random.Int(1, 100) // Random order ID
                    );
                }
                return buyer;
            });
}
