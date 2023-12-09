using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

public class OrderItem
    : Entity
{
    // DDD Patterns comment
    // Using private fields, allowed since EF Core 1.1, is a much better encapsulation
    // aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)

    [field: Required] 
    public string ProductName { get; }

    public string PictureUrl { get; }
    public decimal UnitPrice { get; }
    private decimal _discount;
    public int Units { get; private set; }

    public int ProductId { get; private set; }

    protected OrderItem() { }

    public OrderItem(int productId, string productName, decimal unitPrice, decimal discount, string PictureUrl, int units = 1)
    {
        if (units <= 0)
        {
            throw new OrderingDomainException("Invalid number of units");
        }

        if ((unitPrice * units) < discount)
        {
            throw new OrderingDomainException("The total of order item is lower than applied discount");
        }

        ProductId = productId;

        ProductName = productName;
        UnitPrice = unitPrice;
        _discount = discount;
        Units = units;
        this.PictureUrl = PictureUrl;
    }

    public string GetPictureUri() => PictureUrl;

    public decimal GetCurrentDiscount()
    {
        return _discount;
    }

    public int GetUnits()
    {
        return Units;
    }

    public decimal GetUnitPrice()
    {
        return UnitPrice;
    }

    public string GetOrderItemProductName() => ProductName;

    public void SetNewDiscount(decimal discount)
    {
        if (discount < 0)
        {
            throw new OrderingDomainException("Discount is not valid");
        }

        _discount = discount;
    }

    public void AddUnits(int units)
    {
        if (units < 0)
        {
            throw new OrderingDomainException("Invalid units");
        }

        Units += units;
    }
}
