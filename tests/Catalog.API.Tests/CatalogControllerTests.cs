using Catalog.API.Controllers;
using Catalog.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catalog.API.Tests;

public class CatalogControllerTests
{
    private const string IphoneId = "602d2149e773f2a3990b47f5";

    private readonly FakeProductRepository repository = new();

    private CatalogController CreateController()
    {
        repository.Products.Add(new Product { Id = IphoneId, Name = "IPhone X", Category = "Smart Phone", Price = 950m });
        return new CatalogController(NullLogger<CatalogController>.Instance, repository);
    }

    [Fact]
    public async Task GetById_returns_the_product()
    {
        var result = await CreateController().GetById(IphoneId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var product = Assert.IsType<Product>(ok.Value);
        Assert.Equal("IPhone X", product.Name);
    }

    [Fact]
    public async Task GetById_returns_404_for_an_unknown_product()
    {
        var result = await CreateController().GetById("000000000000000000000000");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_returns_404_when_no_product_matches()
    {
        var result = await CreateController().Update(new Product { Id = "000000000000000000000000", Name = "Ghost" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_returns_404_when_no_product_matches()
    {
        var result = await CreateController().DeleteById("000000000000000000000000");

        Assert.IsType<NotFoundResult>(result);
    }
}
