namespace Basket.API.Services;

public interface IDiscountClient
{
    /// <summary>Discount amount per unit for the product, or 0 when there is no coupon.</summary>
    Task<int> GetDiscountAmountAsync(string productName, CancellationToken cancellationToken = default);
}
