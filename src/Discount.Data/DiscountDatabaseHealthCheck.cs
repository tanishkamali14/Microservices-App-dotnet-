using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Discount.Data;

public class DiscountDatabaseHealthCheck : IHealthCheck
{
    private readonly NpgsqlDataSource dataSource;

    public DiscountDatabaseHealthCheck(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = dataSource.CreateCommand("SELECT 1");
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL is unreachable.", ex);
        }
    }
}
