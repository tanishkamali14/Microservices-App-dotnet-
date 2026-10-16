using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.Data;

/// <summary>
/// Creates the Coupon schema and seeds demo data. Safe to run on every start and from
/// both Discount services at once: it never drops data, every statement is idempotent,
/// and a transaction-scoped advisory lock makes concurrent runs take turns.
/// </summary>
public class DiscountDatabaseMigrator
{
    // Arbitrary application-wide key for pg_advisory_xact_lock.
    private const long SchemaLockKey = 726_354_001;

    private const string Script = """
        CREATE TABLE IF NOT EXISTS Coupon (
            Id          SERIAL PRIMARY KEY,
            ProductName VARCHAR(100) NOT NULL,
            Description TEXT,
            Amount      INT NOT NULL DEFAULT 0
        );

        -- Widen the column on databases created by the original schema (VARCHAR(24)).
        ALTER TABLE Coupon ALTER COLUMN ProductName TYPE VARCHAR(100);

        -- One coupon per product; lookups are by product name.
        CREATE UNIQUE INDEX IF NOT EXISTS ux_coupon_productname ON Coupon (ProductName);

        -- Seed the demo coupons only into an empty table, so deleted coupons stay deleted.
        INSERT INTO Coupon (ProductName, Description, Amount)
        SELECT seed.ProductName, seed.Description, seed.Amount
        FROM (VALUES ('IPhone X', 'IPhone Discount', 150),
                     ('Samsung 10', 'Samsung Discount', 100)) AS seed (ProductName, Description, Amount)
        WHERE NOT EXISTS (SELECT 1 FROM Coupon);
        """;

    private readonly NpgsqlDataSource dataSource;
    private readonly ILogger<DiscountDatabaseMigrator> logger;

    public DiscountDatabaseMigrator(NpgsqlDataSource dataSource, ILogger<DiscountDatabaseMigrator> logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition("SELECT pg_advisory_xact_lock(@Key)", new { Key = SchemaLockKey }, transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(Script, transaction: transaction, cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Discount database schema is up to date.");
    }
}
