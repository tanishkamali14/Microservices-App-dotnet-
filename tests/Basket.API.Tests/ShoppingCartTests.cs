using Basket.API.Entities;

namespace Basket.API.Tests;

public class ShoppingCartTests
{
    [Fact]
    public void TotalPrice_sums_price_times_quantity_for_every_item()
    {
        var cart = new ShoppingCart("swn")
        {
            Items =
            {
                new ShoppingCartItem { ProductId = "a", ProductName = "IPhone X", Price = 10m, Quantity = 2 },
                new ShoppingCartItem { ProductId = "b", ProductName = "Samsung 10", Price = 2.5m, Quantity = 4 }
            }
        };

        Assert.Equal(30m, cart.TotalPrice);
    }

    [Fact]
    public void TotalPrice_of_an_empty_cart_is_zero()
    {
        Assert.Equal(0m, new ShoppingCart("swn").TotalPrice);
    }
}
