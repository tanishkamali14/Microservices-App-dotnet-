using Basket.API.Entities;
using Basket.API.Mapping;

namespace Basket.API.Tests;

public class BasketCheckoutMappingsTests
{
    [Fact]
    public void ToCheckoutEvent_takes_the_total_from_the_basket_not_the_request()
    {
        var checkout = new BasketCheckout
        {
            Username = "swn",
            TotalPrice = 1m, // whatever the client claims
            FirstName = "Ada",
            LastName = "Lovelace",
            EmailAddress = "ada@example.com",
            PaymentMethod = 2
        };

        var message = checkout.ToCheckoutEvent(basketTotal: 950m);

        Assert.Equal(950m, message.TotalPrice);
        Assert.Equal("swn", message.Username);
        Assert.Equal("Ada", message.FirstName);
        Assert.Equal("ada@example.com", message.EmailAddress);
        Assert.Equal(2, message.PaymentMethod);
    }
}
