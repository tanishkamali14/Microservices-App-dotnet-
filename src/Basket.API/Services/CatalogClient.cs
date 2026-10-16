using System.Net;

namespace Basket.API.Services;

public class CatalogClient : ICatalogClient
{
    private readonly HttpClient httpClient;

    public CatalogClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<CatalogProduct?> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/v1/Catalog/{Uri.EscapeDataString(productId)}", cancellationToken);

        // The catalog route only accepts 24-character ids, so malformed ids also come back as 404.
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CatalogProduct>(cancellationToken);
    }
}
