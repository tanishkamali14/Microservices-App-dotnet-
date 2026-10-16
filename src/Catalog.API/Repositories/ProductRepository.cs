using Catalog.API.Data;
using Catalog.API.Entities;
using MongoDB.Driver;

namespace Catalog.API.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ICatalogContext dbContext;

    public ProductRepository(ICatalogContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Add(Product entity)
    {
        await dbContext.Products.InsertOneAsync(entity);
    }

    public async Task<bool> Delete(string id)
    {
        FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(e => e.Id, id);

        DeleteResult deleteResult = await dbContext.Products.DeleteOneAsync(filter);

        return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
    }

    public async Task<IEnumerable<Product>> GetAll()
    {
        return await dbContext.Products.Find(e => true).ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategory(string categoryName)
    {
        FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(e => e.Category, categoryName);
        return await dbContext.Products.Find(filter).ToListAsync();
    }

    public async Task<Product?> GetById(string id)
    {
        return await dbContext.Products.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> GetByName(string name)
    {
        FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(e => e.Name, name);
        return await dbContext.Products.Find(filter).ToListAsync();
    }

    public async Task<bool> Update(Product entity)
    {
        var updateResult = await dbContext.Products
            .ReplaceOneAsync(filter: e => e.Id == entity.Id, replacement: entity);

        // MatchedCount, not ModifiedCount: saving an unchanged product is still a successful update.
        return updateResult.IsAcknowledged && updateResult.MatchedCount > 0;
    }
}
