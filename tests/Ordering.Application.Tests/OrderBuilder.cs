using Ordering.Application.Orders.Commands.CheckoutOrder;

namespace Ordering.Application.Tests;

internal static class OrderBuilder
{
    public static CheckoutOrderCommand ValidCheckout() => new()
    {
        Username = "swn",
        TotalPrice = 950m,
        FirstName = "Ada",
        LastName = "Lovelace",
        EmailAddress = "ada@example.com",
        AddressLine = "1 Main St",
        Country = "United States",
        State = "California",
        ZipCode = "94016",
        CardName = "Ada Lovelace",
        CardNumber = "4111111111111111",
        Expiration = "12/30",
        CVV = "123",
        PaymentMethod = 1
    };
}
