using Ordering.Application.Common.Mappings;

namespace Ordering.Application.Tests;

public class OrderMappingsTests
{
    [Fact]
    public void ToOrder_and_ToViewModel_round_trip_every_field()
    {
        var command = OrderBuilder.ValidCheckout();

        var vm = command.ToOrder().ToViewModel();

        Assert.Equal(command.Username, vm.Username);
        Assert.Equal(command.TotalPrice, vm.TotalPrice);
        Assert.Equal(command.FirstName, vm.FirstName);
        Assert.Equal(command.LastName, vm.LastName);
        Assert.Equal(command.EmailAddress, vm.EmailAddress);
        Assert.Equal(command.AddressLine, vm.AddressLine);
        Assert.Equal(command.Country, vm.Country);
        Assert.Equal(command.State, vm.State);
        Assert.Equal(command.ZipCode, vm.ZipCode);
        Assert.Equal(command.CardName, vm.CardName);
        Assert.Equal(command.PaymentMethod, vm.PaymentMethod);
    }
}
