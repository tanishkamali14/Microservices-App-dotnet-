using Microsoft.Extensions.Logging.Abstractions;
using Ordering.Application.Common.Exceptions;
using Ordering.Application.Common.Mappings;
using Ordering.Application.Orders.Commands.DeleteOrder;
using Ordering.Application.Orders.Commands.UpdateOrder;
using Ordering.Application.Tests.Fakes;

namespace Ordering.Application.Tests;

public class OrderCommandHandlerTests
{
    private readonly FakeOrderRepository repository = new();

    [Fact]
    public async Task Deleting_an_unknown_order_throws_NotFoundException()
    {
        var handler = new DeleteOrderCommandHandler(repository, NullLogger<DeleteOrderCommandHandler>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new DeleteOrderCommand { Id = 42 }, CancellationToken.None));
    }

    [Fact]
    public async Task Deleting_an_existing_order_removes_it()
    {
        var order = await repository.AddAsync(OrderBuilder.ValidCheckout().ToOrder());
        var handler = new DeleteOrderCommandHandler(repository, NullLogger<DeleteOrderCommandHandler>.Instance);

        await handler.Handle(new DeleteOrderCommand { Id = order.Id }, CancellationToken.None);

        Assert.Empty(repository.Orders);
    }

    [Fact]
    public async Task Updating_an_unknown_order_throws_NotFoundException()
    {
        var handler = new UpdateOrderCommandHandler(repository, NullLogger<UpdateOrderCommandHandler>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new UpdateOrderCommand { Id = 7, Username = "swn" }, CancellationToken.None));
    }

    [Fact]
    public async Task Updating_an_order_copies_the_new_values_but_keeps_its_id()
    {
        var order = await repository.AddAsync(OrderBuilder.ValidCheckout().ToOrder());
        var handler = new UpdateOrderCommandHandler(repository, NullLogger<UpdateOrderCommandHandler>.Instance);

        await handler.Handle(new UpdateOrderCommand
        {
            Id = order.Id,
            Username = "swn",
            TotalPrice = 100m,
            FirstName = "Grace",
            EmailAddress = "grace@example.com"
        }, CancellationToken.None);

        var updated = Assert.Single(repository.Orders);
        Assert.Equal(order.Id, updated.Id);
        Assert.Equal("Grace", updated.FirstName);
        Assert.Equal(100m, updated.TotalPrice);
    }
}
