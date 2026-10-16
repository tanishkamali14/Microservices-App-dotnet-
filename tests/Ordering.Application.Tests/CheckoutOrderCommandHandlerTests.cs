using Microsoft.Extensions.Logging.Abstractions;
using Ordering.Application.Orders.Commands.CheckoutOrder;
using Ordering.Application.Tests.Fakes;

namespace Ordering.Application.Tests;

public class CheckoutOrderCommandHandlerTests
{
    private readonly FakeOrderRepository repository = new();
    private readonly FakeEmailService email = new();

    private CheckoutOrderCommandHandler CreateHandler() =>
        new(repository, email, NullLogger<CheckoutOrderCommandHandler>.Instance);

    [Fact]
    public async Task Creates_an_order_from_the_command_and_returns_its_id()
    {
        var id = await CreateHandler().Handle(OrderBuilder.ValidCheckout(), CancellationToken.None);

        var order = Assert.Single(repository.Orders);
        Assert.Equal(id, order.Id);
        Assert.Equal("swn", order.Username);
        Assert.Equal(950m, order.TotalPrice);
        Assert.Equal("Ada", order.FirstName);
        Assert.Equal(1, order.PaymentMethod);
    }

    [Fact]
    public async Task Sends_the_confirmation_to_the_address_on_the_order()
    {
        await CreateHandler().Handle(OrderBuilder.ValidCheckout(), CancellationToken.None);

        var sent = Assert.Single(email.Sent);
        Assert.Equal("ada@example.com", sent.To);
    }

    [Fact]
    public async Task A_failing_email_service_does_not_fail_the_order()
    {
        email.ThrowOnSend = true;

        var id = await CreateHandler().Handle(OrderBuilder.ValidCheckout(), CancellationToken.None);

        Assert.True(id > 0);
        Assert.Single(repository.Orders);
    }
}
