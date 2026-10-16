namespace Catalog.API.Data;

/// <summary>
/// Seeds the catalog once at startup without blocking the app from starting. MongoDB may
/// still be booting when the container starts, so failed attempts are retried with backoff.
/// </summary>
public class CatalogSeeder : BackgroundService
{
    private const int MaxAttempts = 10;

    private readonly ICatalogContext context;
    private readonly ILogger<CatalogSeeder> logger;

    public CatalogSeeder(ICatalogContext context, ILogger<CatalogSeeder> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                var seeded = await CatalogContextSeed.SeedAsync(context.Products, stoppingToken);
                logger.LogInformation(seeded ? "Catalog seeded with demo products." : "Catalog already contains products; seeding skipped.");
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, attempt)));
                logger.LogWarning(ex, "Seeding attempt {Attempt}/{MaxAttempts} failed; retrying in {Delay}.", attempt, MaxAttempts, delay);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
