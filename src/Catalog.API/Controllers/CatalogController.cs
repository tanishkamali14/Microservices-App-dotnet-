using System.Net;
using Catalog.API.Entities;
using Catalog.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ILogger<CatalogController> logger;
    private readonly IProductRepository repository;

    public CatalogController(ILogger<CatalogController> logger, IProductRepository repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var list = await repository.GetAll();
        return Ok(list);
    }

    [HttpGet("{id:length(24)}", Name = "GetProduct")]
    [ProducesResponseType(typeof(Product), (int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    public async Task<ActionResult<Product>> GetById(string id)
    {
        var entity = await repository.GetById(id);
        if (entity == null)
        {
            logger.LogWarning("Product with id {ProductId} not found.", id);
            return NotFound();
        }
        return Ok(entity);
    }

    [HttpGet]
    [Route("[action]/{category}", Name = "GetByCategory")]
    [ProducesResponseType(typeof(IEnumerable<Product>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<Product>>> GetByCategory(string category)
    {
        var list = await repository.GetByCategory(category);
        return Ok(list);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Product), (int) HttpStatusCode.Created)]
    public async Task<ActionResult<Product>> Add([FromBody] Product product)
    {
        await repository.Add(product);
        return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
    }

    [HttpPut]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    public async Task<IActionResult> Update([FromBody] Product product)
    {
        var updated = await repository.Update(product);
        return updated ? Ok(true) : NotFound();
    }

    [HttpDelete("{id:length(24)}", Name = "DeleteProduct")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeleteById(string id)
    {
        var deleted = await repository.Delete(id);
        return deleted ? Ok(true) : NotFound();
    }
}
