using Discount.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;

namespace Discount.Data;

public static class DiscountDataExtensions
{
    public static IServiceCollection AddDiscountData(this IServiceCollection services, string connectionString)
    {
        // One data source per app: it owns the connection pool.
        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
        services.AddScoped<IDiscountRepository, DiscountRepository>();
        services.AddSingleton<DiscountDatabaseMigrator>();
        return services;
    }

    /// <summary>
    /// Runs the migration, retrying with exponential backoff while PostgreSQL is still starting.
    /// Replaces the old recursive Thread.Sleep loop that blocked startup and swallowed the
    /// final failure; if the database never comes up, this now fails startup loudly.
    /// </summary>
    public static async Task MigrateDiscountDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var migrator = services.GetRequiredService<DiscountDatabaseMigrator>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DiscountDataExtensions));

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                // Only retry errors that can go away on their own (server still starting, connection refused).
                // A wrong or missing password fails immediately instead of after a minute of retries.
                ShouldHandle = new PredicateBuilder().Handle<NpgsqlException>(ex => ex.IsTransient).Handle<TimeoutException>(),
                MaxRetryAttempts = 8,
                Delay = TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromSeconds(15),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(args.Outcome.Exception,
                        "Discount database is not ready (attempt {Attempt}); retrying in {Delay}.",
                        args.AttemptNumber + 1, args.RetryDelay);
                    return default;
                }
            })
            .Build();

        await pipeline.ExecuteAsync(async ct => await migrator.MigrateAsync(ct), cancellationToken);
    }
}
