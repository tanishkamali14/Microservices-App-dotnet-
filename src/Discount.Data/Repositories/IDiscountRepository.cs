using Discount.Data.Entities;

namespace Discount.Data.Repositories;

public interface IDiscountRepository
{
    /// <returns>The coupon, or <c>null</c> when the product has no discount.</returns>
    Task<Coupon?> GetDiscount(string productName);

    /// <returns><c>false</c> when a coupon for the product already exists.</returns>
    Task<bool> CreateDiscount(Coupon coupon);

    /// <returns><c>false</c> when no coupon has the given id.</returns>
    Task<bool> UpdateDiscount(Coupon coupon);

    /// <returns><c>false</c> when the product had no coupon.</returns>
    Task<bool> DeleteDiscount(string productName);
}
