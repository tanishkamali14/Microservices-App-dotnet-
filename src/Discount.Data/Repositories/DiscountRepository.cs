using Dapper;
using Discount.Data.Entities;
using Npgsql;

namespace Discount.Data.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly NpgsqlDataSource dataSource;

    public DiscountRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<Coupon?> GetDiscount(string productName)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<Coupon>(
            "SELECT Id, ProductName, Description, Amount FROM Coupon WHERE ProductName = @ProductName",
            new { ProductName = productName });
    }

    public async Task<bool> CreateDiscount(Coupon coupon)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        var affected = await connection.ExecuteAsync(
            "INSERT INTO Coupon (ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount) ON CONFLICT (ProductName) DO NOTHING",
            new { coupon.ProductName, coupon.Description, coupon.Amount });

        return affected != 0;
    }

    public async Task<bool> UpdateDiscount(Coupon coupon)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        var affected = await connection.ExecuteAsync(
            "UPDATE Coupon SET ProductName = @ProductName, Description = @Description, Amount = @Amount WHERE Id = @Id",
            new { coupon.ProductName, coupon.Description, coupon.Amount, coupon.Id });

        return affected != 0;
    }

    public async Task<bool> DeleteDiscount(string productName)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        var affected = await connection.ExecuteAsync(
            "DELETE FROM Coupon WHERE ProductName = @ProductName",
            new { ProductName = productName });

        return affected != 0;
    }
}
