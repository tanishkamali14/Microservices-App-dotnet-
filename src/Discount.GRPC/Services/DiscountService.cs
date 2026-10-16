using Discount.GRPC.Mapping;
using Discount.GRPC.Protos;
using Discount.GRPC.Repositories;
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
        var coupon = await repository.GetDiscount(request.ProductName);

        if (coupon == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Discount with ProductName={request.ProductName} is not found."));
        }

        return coupon.ToModel();
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.ToEntity();
        await repository.CreateDiscount(coupon);
        logger.LogInformation("Discount is successfully created. ProductName : {ProductName}", coupon.ProductName);

        return coupon.ToModel();
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.ToEntity();

        await repository.UpdateDiscount(coupon);
        logger.LogInformation("Discount is successfully updated. ProductName : {ProductName}", coupon.ProductName);

        return coupon.ToModel();
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        var deleted = await repository.DeleteDiscount(request.ProductName);
        var response = new DeleteDiscountResponse
        {
            Success = deleted
        };

        return response;
    }
}
