using MediatR;
using Ordering.Application.Common.Contracts.Persistence;
using Ordering.Application.Common.Mappings;

namespace Ordering.Application.Orders.Queries.GetOrdersList;

public record GetOrdersListQuery : IRequest<List<OrdersVm>>
{
    public string Username { get; set; } = null!;
}

public class GetOrdersListQueryHandler : IRequestHandler<GetOrdersListQuery, List<OrdersVm>>
{
    private readonly IOrderRepository orderRepository;

    public GetOrdersListQueryHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public async Task<List<OrdersVm>> Handle(GetOrdersListQuery request, CancellationToken cancellationToken)
    {
        var orderList = await orderRepository.GetOrdersByUserName(request.Username);
        return orderList.Select(order => order.ToViewModel()).ToList();
    }
}
