using Basket.API.Entities;

namespace Basket.API.Services;

/// <summary>
/// Prices every line of a basket from the Catalog and Discount services.
/// Prices sent by the client are ignored, and because each run starts from the catalog
/// price, pricing the same basket again never applies a discount twice.
/// </summary>
public class BasketPricingService
{
    public const int MaxQuantity = 100;

    private readonly ICatalogClient catalog;
    private readonly IDiscountClient discounts;

    public BasketPricingService(ICatalogClient catalog, IDiscountClient discounts)
    {
        this.catalog = catalog;
        this.discounts = discounts;
    }

    public async Task PriceAsync(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        foreach (var item in basket.Items)
        {
            if (item.Quantity is < 1 or > MaxQuantity)
            {
                throw new InvalidBasketException($"Quantity for product '{item.ProductId}' must be between 1 and {MaxQuantity}.");
            }

            var product = await catalog.GetProductAsync(item.ProductId, cancellationToken)
                ?? throw new InvalidBasketException($"Product '{item.ProductId}' does not exist.");

            var discount = await discounts.GetDiscountAmountAsync(product.Name, cancellationToken);

            item.ProductName = product.Name;
            item.UnitPrice = product.Price;
            item.Discount = Math.Clamp(discount, 0m, product.Price);
            item.Price = item.UnitPrice - item.Discount;
        }
    }
}
