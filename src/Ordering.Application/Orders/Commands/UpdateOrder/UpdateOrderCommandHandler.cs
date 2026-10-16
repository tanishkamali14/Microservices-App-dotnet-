using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Contracts.Persistence;
using Ordering.Application.Common.Exceptions;
using Ordering.Application.Common.Mappings;
using Ordering.Domain.Entities;

namespace Ordering.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand : IRequest
{
    public int Id { get; set; }
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

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand>
{
    private readonly IOrderRepository orderRepository;
    private readonly ILogger<UpdateOrderCommandHandler> logger;

    public UpdateOrderCommandHandler(IOrderRepository orderRepository, ILogger<UpdateOrderCommandHandler> logger)
    {
        this.orderRepository = orderRepository;
        this.logger = logger;
    }

    public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderToUpdate = await orderRepository.GetByIdAsync(request.Id);
        if (orderToUpdate == null)
        {
            throw new NotFoundException(nameof(Order), request.Id);
        }

        request.ApplyTo(orderToUpdate);
        await orderRepository.UpdateAsync(orderToUpdate);
        logger.LogInformation("Order {OrderId} is successfully updated.", orderToUpdate.Id);
    }
}
