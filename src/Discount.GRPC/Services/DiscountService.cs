using Discount.Data.Entities;
using Discount.Data.Repositories;
using Discount.GRPC.Mapping;
using Discount.GRPC.Protos;
using Grpc.Core;

namespace Discount.GRPC.Services;

public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
{
    private readonly IDiscountRepository repository;
    private readonly ILogger<DiscountService> logger;

    public DiscountService(IDiscountRepository repository, ILogger<DiscountService> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        // A product without a coupon simply has no discount, so Basket gets an amount of 0.
        var coupon = await repository.GetDiscount(request.ProductName) ?? Coupon.NoDiscount(request.ProductName);

        return coupon.ToModel();
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.ToEntity();
        if (!await repository.CreateDiscount(coupon))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"A discount for '{coupon.ProductName}' already exists."));
        }

        logger.LogInformation("Discount is successfully created. ProductName : {ProductName}", coupon.ProductName);
        return coupon.ToModel();
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.ToEntity();
        if (!await repository.UpdateDiscount(coupon))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Discount with Id={coupon.Id} is not found."));
        }

        logger.LogInformation("Discount is successfully updated. ProductName : {ProductName}", coupon.ProductName);
        return coupon.ToModel();
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        var deleted = await repository.DeleteDiscount(request.ProductName);
        return new DeleteDiscountResponse
        {
            Success = deleted
        };
    }
}
