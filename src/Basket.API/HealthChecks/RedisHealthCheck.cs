using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Basket.API.HealthChecks;

/// <summary>Round-trips through the same IDistributedCache the baskets use.</summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IDistributedCache cache;

    public RedisHealthCheck(IDistributedCache cache)
    {
        this.cache = cache;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.GetStringAsync("health-probe", cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is unreachable.", ex);
        }
    }
}
