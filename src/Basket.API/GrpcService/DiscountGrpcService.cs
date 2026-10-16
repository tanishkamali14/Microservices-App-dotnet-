using Basket.API.Services;
using Discount.GRPC.Protos;

namespace Basket.API.GrpcService;

public class DiscountGrpcService : IDiscountClient
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly DiscountProtoService.DiscountProtoServiceClient discountProtoService;

    public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoService)
    {
        this.discountProtoService = discountProtoService;
    }

    public async Task<int> GetDiscountAmountAsync(string productName, CancellationToken cancellationToken = default)
    {
        var request = new GetDiscountRequest
        {
            ProductName = productName
        };

        var coupon = await discountProtoService.GetDiscountAsync(request, deadline: DateTime.UtcNow.Add(Timeout), cancellationToken: cancellationToken);
        return coupon.Amount;
    }
}
