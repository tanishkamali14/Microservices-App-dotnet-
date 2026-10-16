using Basket.API.Entities;
using Basket.API.Services;

namespace Basket.API.Tests;

public class BasketPricingServiceTests
{
    private const string IphoneId = "602d2149e773f2a3990b47f5";
    private const string LgId = "602d2149e773f2a3990b47fa";

    private readonly FakeCatalogClient catalog = new()
    {
        Products =
        {
            [IphoneId] = new CatalogProduct(IphoneId, "IPhone X", 950m),
            [LgId] = new CatalogProduct(LgId, "LG G7 ThinQ", 240m)
        }
    };

    private readonly FakeDiscountClient discounts = new()
    {
        Amounts = { ["IPhone X"] = 150 }
    };

    private BasketPricingService CreateService() => new(catalog, discounts);

    private static ShoppingCart BasketWith(string productId, int quantity = 1, decimal clientPrice = 0m) => new("swn")
    {
        Items = { new ShoppingCartItem { ProductId = productId, Quantity = quantity, Price = clientPrice, ProductName = "client supplied" } }
    };

    [Fact]
    public async Task Applies_the_discount_to_the_catalog_price()
    {
        var basket = BasketWith(IphoneId, quantity: 2);

        await CreateService().PriceAsync(basket);

        var item = Assert.Single(basket.Items);
        Assert.Equal(950m, item.UnitPrice);
        Assert.Equal(150m, item.Discount);
        Assert.Equal(800m, item.Price);
        Assert.Equal(1600m, basket.TotalPrice);
    }

    [Fact]
    public async Task Saving_the_same_basket_twice_does_not_stack_the_discount()
    {
        // Regression test: the original UpdateBasket did `item.Price -= coupon.Amount`
        // on whatever price came back from the client, so every save took another 150 off.
        var basket = BasketWith(IphoneId);
        var service = CreateService();

        await service.PriceAsync(basket);
        await service.PriceAsync(basket);
        await service.PriceAsync(basket);

        Assert.Equal(800m, Assert.Single(basket.Items).Price);
    }

    [Fact]
    public async Task Ignores_prices_and_names_sent_by_the_client()
    {
        var basket = BasketWith(LgId, clientPrice: 0.01m);

        await CreateService().PriceAsync(basket);

        var item = Assert.Single(basket.Items);
        Assert.Equal(240m, item.Price);
        Assert.Equal("LG G7 ThinQ", item.ProductName);
    }

    [Fact]
    public async Task A_discount_larger_than_the_price_brings_the_price_to_zero_not_below()
    {
        discounts.Amounts["LG G7 ThinQ"] = 1000;
        var basket = BasketWith(LgId);

        await CreateService().PriceAsync(basket);

        Assert.Equal(0m, Assert.Single(basket.Items).Price);
    }

    [Fact]
    public async Task Unknown_products_are_rejected()
    {
        var basket = BasketWith("000000000000000000000000");

        await Assert.ThrowsAsync<InvalidBasketException>(() => CreateService().PriceAsync(basket));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(BasketPricingService.MaxQuantity + 1)]
    public async Task Quantities_outside_the_allowed_range_are_rejected(int quantity)
    {
        var basket = BasketWith(IphoneId, quantity);

        await Assert.ThrowsAsync<InvalidBasketException>(() => CreateService().PriceAsync(basket));
    }

    private sealed class FakeCatalogClient : ICatalogClient
    {
        public Dictionary<string, CatalogProduct> Products { get; } = new();

        public Task<CatalogProduct?> GetProductAsync(string productId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Products.GetValueOrDefault(productId));
    }

    private sealed class FakeDiscountClient : IDiscountClient
    {
        public Dictionary<string, int> Amounts { get; } = new();

        public Task<int> GetDiscountAmountAsync(string productName, CancellationToken cancellationToken = default) =>
            Task.FromResult(Amounts.GetValueOrDefault(productName));
    }
}
