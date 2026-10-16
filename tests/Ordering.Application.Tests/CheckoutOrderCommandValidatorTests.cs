using Ordering.Application.Orders.Commands.CheckoutOrder;

namespace Ordering.Application.Tests;

public class CheckoutOrderCommandValidatorTests
{
    private readonly CheckoutOrderCommandValidator validator = new();

    [Fact]
    public void A_complete_checkout_is_valid()
    {
        var result = validator.Validate(OrderBuilder.ValidCheckout());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("this-user-name-is-definitely-longer-than-fifty-characters")]
    public void Username_must_be_present_and_at_most_50_characters(string username)
    {
        var command = OrderBuilder.ValidCheckout() with { Username = username };

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CheckoutOrderCommand.Username));
    }

    [Fact]
    public void Email_address_is_required()
    {
        var command = OrderBuilder.ValidCheckout() with { EmailAddress = "" };

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CheckoutOrderCommand.EmailAddress));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Total_price_must_be_positive(int total)
    {
        var command = OrderBuilder.ValidCheckout() with { TotalPrice = total };

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CheckoutOrderCommand.TotalPrice));
    }
}
