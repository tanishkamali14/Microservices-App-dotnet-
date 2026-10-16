using EventBus.Messages.Events;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.CheckoutOrder;

namespace Ordering.API.EventBusConsumer;

public class BasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
{
    private readonly IMediator mediator;
    private readonly ILogger<BasketCheckoutConsumer> logger;

    public BasketCheckoutConsumer(IMediator mediator, ILogger<BasketCheckoutConsumer> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        var command = ToCommand(context.Message);
        var result = await mediator.Send(command, context.CancellationToken);

        logger.LogInformation("BasketCheckoutEvent consumed successfully. Created Order Id: {NewOrderId}", result);
    }

    public static CheckoutOrderCommand ToCommand(BasketCheckoutEvent message) => new()
    {
        Username = message.Username,
        TotalPrice = message.TotalPrice,
        FirstName = message.FirstName,
        LastName = message.LastName,
        EmailAddress = message.EmailAddress,
        AddressLine = message.AddressLine,
        Country = message.Country,
        State = message.State,
        ZipCode = message.ZipCode,
        CardName = message.CardName,
        CardNumber = message.CardNumber,
        Expiration = message.Expiration,
        CVV = message.CVV,
        PaymentMethod = message.PaymentMethod
    };
}
