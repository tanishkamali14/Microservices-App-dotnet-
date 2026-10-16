using FluentValidation;
using MediatR;
using Ordering.Application.Common.Behaviours;
using Ordering.Application.Orders.Commands.CheckoutOrder;
using ValidationException = Ordering.Application.Common.Exceptions.ValidationException;

namespace Ordering.Application.Tests;

public class ValidationBehaviorTests
{
    private static ValidationBehavior<CheckoutOrderCommand, int> CreateBehavior() =>
        new(new IValidator<CheckoutOrderCommand>[] { new CheckoutOrderCommandValidator() });

    [Fact]
    public async Task Invalid_requests_never_reach_the_handler()
    {
        var handlerCalled = false;
        RequestHandlerDelegate<int> next = _ =>
        {
            handlerCalled = true;
            return Task.FromResult(1);
        };
        var invalid = OrderBuilder.ValidCheckout() with { Username = "", TotalPrice = 0 };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => CreateBehavior().Handle(invalid, next, CancellationToken.None));

        Assert.False(handlerCalled);
        Assert.Contains(nameof(CheckoutOrderCommand.Username), ex.Errors);
        Assert.Contains(nameof(CheckoutOrderCommand.TotalPrice), ex.Errors);
    }

    [Fact]
    public async Task Valid_requests_are_passed_to_the_handler()
    {
        RequestHandlerDelegate<int> next = _ => Task.FromResult(42);

        var result = await CreateBehavior().Handle(OrderBuilder.ValidCheckout(), next, CancellationToken.None);

        Assert.Equal(42, result);
    }
}
