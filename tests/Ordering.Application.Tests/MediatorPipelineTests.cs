using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Common.Contracts.Notifications;
using Ordering.Application.Common.Contracts.Persistence;
using Ordering.Application.Orders.Commands.UpdateOrder;
using Ordering.Application.Orders.Queries.GetOrdersList;
using Ordering.Application.Tests.Fakes;
using ValidationException = Ordering.Application.Common.Exceptions.ValidationException;

namespace Ordering.Application.Tests;

/// <summary>
/// Runs commands through the real MediatR registration so the pipeline behaviours,
/// validators and handlers are wired exactly as in the API.
/// </summary>
public class MediatorPipelineTests
{
    private readonly FakeOrderRepository repository = new();

    private IMediator CreateMediator()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();
        services.AddSingleton<IOrderRepository>(repository);
        services.AddSingleton<IEmailService>(new FakeEmailService());
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Checkout_then_query_returns_the_new_order()
    {
        var mediator = CreateMediator();

        await mediator.Send(OrderBuilder.ValidCheckout());
        var orders = await mediator.Send(new GetOrdersListQuery { Username = "swn" });

        var order = Assert.Single(orders);
        Assert.Equal(950m, order.TotalPrice);
    }

    [Fact]
    public async Task Validation_runs_for_commands_without_a_response_type()
    {
        // Behaviours constrained to IRequest<TResponse> would silently skip void commands
        // such as UpdateOrderCommand after the MediatR 12 upgrade.
        var mediator = CreateMediator();

        await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(new UpdateOrderCommand { Id = 1, Username = "" }));
    }
}
