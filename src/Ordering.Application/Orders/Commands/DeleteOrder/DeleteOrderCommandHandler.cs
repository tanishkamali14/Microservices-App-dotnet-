using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Contracts.Persistence;
using Ordering.Application.Common.Exceptions;
using Ordering.Domain.Entities;

namespace Ordering.Application.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand : IRequest
{
    public int Id { get; set; }
}

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
{
    private readonly IOrderRepository orderRepository;
    private readonly ILogger<DeleteOrderCommandHandler> logger;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository, ILogger<DeleteOrderCommandHandler> logger)
    {
        this.orderRepository = orderRepository;
        this.logger = logger;
    }

    public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var orderToDelete = await orderRepository.GetByIdAsync(request.Id);

        if (orderToDelete == null)
        {
            throw new NotFoundException(nameof(Order), request.Id);
        }

        await orderRepository.DeleteAsync(orderToDelete);
        logger.LogInformation("Order {OrderId} is successfully deleted.", orderToDelete.Id);
    }
}
