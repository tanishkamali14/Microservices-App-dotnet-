using Ordering.Application.Orders.Commands.CheckoutOrder;
using Ordering.Application.Orders.Commands.UpdateOrder;
using Ordering.Application.Orders.Queries.GetOrdersList;
using Ordering.Domain.Entities;

namespace Ordering.Application.Common.Mappings;

/// <summary>
/// Explicit mappings between commands, entities and view models. Replaces the
/// former AutoMapper profile so every mapped field is visible and checked by the compiler.
/// </summary>
public static class OrderMappings
{
    public static Order ToOrder(this CheckoutOrderCommand command) => new()
    {
        Username = command.Username,
        TotalPrice = command.TotalPrice,
        FirstName = command.FirstName,
        LastName = command.LastName,
        EmailAddress = command.EmailAddress,
        AddressLine = command.AddressLine,
        Country = command.Country,
        State = command.State,
        ZipCode = command.ZipCode,
        CardName = command.CardName,
        CardNumber = command.CardNumber,
        Expiration = command.Expiration,
        CVV = command.CVV,
        PaymentMethod = command.PaymentMethod
    };

    public static void ApplyTo(this UpdateOrderCommand command, Order order)
    {
        order.Username = command.Username;
        order.TotalPrice = command.TotalPrice;
        order.FirstName = command.FirstName;
        order.LastName = command.LastName;
        order.EmailAddress = command.EmailAddress;
        order.AddressLine = command.AddressLine;
        order.Country = command.Country;
        order.State = command.State;
        order.ZipCode = command.ZipCode;
        order.CardName = command.CardName;
        order.CardNumber = command.CardNumber;
        order.Expiration = command.Expiration;
        order.CVV = command.CVV;
        order.PaymentMethod = command.PaymentMethod;
    }

    public static OrdersVm ToViewModel(this Order order) => new()
    {
        Username = order.Username,
        TotalPrice = order.TotalPrice,
        FirstName = order.FirstName,
        LastName = order.LastName,
        EmailAddress = order.EmailAddress,
        AddressLine = order.AddressLine,
        Country = order.Country,
        State = order.State,
        ZipCode = order.ZipCode,
        CardName = order.CardName,
        CardNumber = order.CardNumber,
        Expiration = order.Expiration,
        CVV = order.CVV,
        PaymentMethod = order.PaymentMethod
    };
}
