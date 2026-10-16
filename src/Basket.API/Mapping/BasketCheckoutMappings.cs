using Basket.API.Entities;
using EventBus.Messages.Events;

namespace Basket.API.Mapping;

public static class BasketCheckoutMappings
{
    /// <summary>
    /// Builds the integration event for a checkout. The total always comes from the
    /// stored basket, never from the request body.
    /// </summary>
    public static BasketCheckoutEvent ToCheckoutEvent(this BasketCheckout checkout, decimal basketTotal) => new()
    {
        Username = checkout.Username,
        TotalPrice = basketTotal,
        FirstName = checkout.FirstName,
        LastName = checkout.LastName,
        EmailAddress = checkout.EmailAddress,
        AddressLine = checkout.AddressLine,
        Country = checkout.Country,
        State = checkout.State,
        ZipCode = checkout.ZipCode,
        CardName = checkout.CardName,
        CardNumber = checkout.CardNumber,
        Expiration = checkout.Expiration,
        CVV = checkout.CVV,
        PaymentMethod = checkout.PaymentMethod
    };
}
