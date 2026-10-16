namespace Basket.API.Entities;

public class ShoppingCartItem
{
    public int Quantity { get; set; }
    public string Color { get; set; } = "";

    /// <summary>Catalog price for one unit, before any discount. Set by the server.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Discount per unit from the Discount service. Set by the server.</summary>
    public decimal Discount { get; set; }

    /// <summary>Price per unit after the discount (UnitPrice - Discount). Set by the server.</summary>
    public decimal Price { get; set; }

    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
}
