namespace Discount.Data.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public string Description { get; set; } = "";
    public int Amount { get; set; }

    /// <summary>The coupon returned when a product has no discount.</summary>
    public static Coupon NoDiscount(string productName) => new()
    {
        ProductName = productName,
        Description = "No discount",
        Amount = 0
    };
}
