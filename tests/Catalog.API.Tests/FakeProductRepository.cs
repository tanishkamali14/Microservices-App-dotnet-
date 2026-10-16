using Catalog.API.Entities;
using Catalog.API.Repositories;

namespace Catalog.API.Tests;

public class FakeProductRepository : IProductRepository
{
    public List<Product> Products { get; } = new();

    public Task<IEnumerable<Product>> GetAll() => Task.FromResult<IEnumerable<Product>>(Products.ToList());

    public Task<Product?> GetById(string id) => Task.FromResult(Products.FirstOrDefault(p => p.Id == id));

    public Task<IEnumerable<Product>> GetByName(string name) => Task.FromResult<IEnumerable<Product>>(Products.Where(p => p.Name == name).ToList());

    public Task<IEnumerable<Product>> GetByCategory(string categoryName) => Task.FromResult<IEnumerable<Product>>(Products.Where(p => p.Category == categoryName).ToList());

    public Task Add(Product entity)
    {
        Products.Add(entity);
        return Task.CompletedTask;
    }

    public Task<bool> Update(Product entity)
    {
        var index = Products.FindIndex(p => p.Id == entity.Id);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        Products[index] = entity;
        return Task.FromResult(true);
    }

    public Task<bool> Delete(string id) => Task.FromResult(Products.RemoveAll(p => p.Id == id) > 0);
}
