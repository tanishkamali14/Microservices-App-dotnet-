using Catalog.API.Entities;
using MongoDB.Driver;

namespace Catalog.API.Data;

/// <summary>
/// Wraps the products collection. Registered as a singleton together with
/// <see cref="IMongoClient"/>: the driver pools connections inside the client, so
/// creating a client per request (as before) opened a new pool on every call.
/// </summary>
public class CatalogContext : ICatalogContext
{
    public IMongoCollection<Product> Products { get; }

    public CatalogContext(IMongoClient client, IConfiguration configuration)
    {
        var database = client.GetDatabase(configuration["DatabaseSettings:DatabaseName"]);

        Products = database.GetCollection<Product>(configuration["DatabaseSettings:CollectionName"]);
    }
}
