using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace Ordering.API.Extensions;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Applies EF Core migrations and runs the seeder, retrying with exponential backoff while
    /// SQL Server is still starting. Replaces the old recursive Thread.Sleep loop, which blocked
    /// a thread, swallowed the final failure and let the API start without a database.
    /// </summary>
    public static async Task MigrateDatabaseAsync<TContext>(this IServiceProvider services,
        Func<TContext, IServiceProvider, Task> seeder,
        CancellationToken cancellationToken = default) where TContext : DbContext
    {
        var logger = services.GetRequiredService<ILogger<TContext>>();

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<DbException>(),
                MaxRetryAttempts = 10,
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(20),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(args.Outcome.Exception,
                        "Database for {DbContextName} is not ready (attempt {Attempt}); retrying in {Delay}.",
                        typeof(TContext).Name, args.AttemptNumber + 1, args.RetryDelay);
                    return default;
                }
            })
            .Build();

        await pipeline.ExecuteAsync(async ct =>
        {
            await using var scope = services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);
            await context.Database.MigrateAsync(ct);
            await seeder(context, scope.ServiceProvider);
        }, cancellationToken);
    }
}
