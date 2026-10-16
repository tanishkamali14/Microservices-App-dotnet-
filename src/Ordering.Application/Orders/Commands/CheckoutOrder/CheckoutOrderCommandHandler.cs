using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Contracts.Notifications;
using Ordering.Application.Common.Contracts.Persistence;
using Ordering.Application.Common.Mappings;
using Ordering.Application.Common.Models;

namespace Ordering.Application.Orders.Commands.CheckoutOrder;

public record CheckoutOrderCommand : IRequest<int>
{
    public string Username { get; set; } = "";
    public decimal TotalPrice { get; set; }

    // Billing Address
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public string AddressLine { get; set; } = "";
    public string Country { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";

    // Payment
    public string CardName { get; set; } = "";
    public string CardNumber { get; set; } = "";
    public string Expiration { get; set; } = "";
    public string CVV { get; set; } = "";
    public int PaymentMethod { get; set; }
}

public class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, int>
{
    private readonly IOrderRepository orderRepository;
    private readonly IEmailService emailService;
    private readonly ILogger<CheckoutOrderCommandHandler> logger;

    public CheckoutOrderCommandHandler(IOrderRepository orderRepository, IEmailService emailService, ILogger<CheckoutOrderCommandHandler> logger)
    {
        this.orderRepository = orderRepository;
        this.emailService = emailService;
        this.logger = logger;
    }

    public async Task<int> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        var orderEntity = request.ToOrder();
        var newOrder = await orderRepository.AddAsync(orderEntity);

        logger.LogInformation("Order {OrderId} is successfully created.", newOrder.Id);

        var email = new Email()
        {
            To = newOrder.EmailAddress,
            Subject = $"Your order #{newOrder.Id}",
            Body = $"Thanks {newOrder.FirstName}, we received your order #{newOrder.Id} for {newOrder.TotalPrice:C}."
        };

        try
        {
            await emailService.SendEmail(email);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Order {OrderId} failed due to an error with the mail service", newOrder.Id);
        }

        return newOrder.Id;
    }
}
