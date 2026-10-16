using Discount.GRPC.Entities;
using Discount.GRPC.Protos;

namespace Discount.GRPC.Mapping;

public static class CouponMappings
{
    public static CouponModel ToModel(this Coupon coupon) => new()
    {
        Id = coupon.Id,
        ProductName = coupon.ProductName,
        Description = coupon.Description,
        Amount = coupon.Amount
    };

    public static Coupon ToEntity(this CouponModel model) => new()
    {
        Id = model.Id,
        ProductName = model.ProductName,
        Description = model.Description,
        Amount = model.Amount
    };
}
