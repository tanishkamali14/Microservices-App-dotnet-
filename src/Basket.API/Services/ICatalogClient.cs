namespace Basket.API.Services;

public record CatalogProduct(string Id, string Name, decimal Price);

/// <summary>Reads authoritative product data from the Catalog service.</summary>
public interface ICatalogClient
{
    Task<CatalogProduct?> GetProductAsync(string productId, CancellationToken cancellationToken = default);
}
