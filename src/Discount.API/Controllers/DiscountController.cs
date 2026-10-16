using Discount.Data.Entities;
using Discount.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Discount.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DiscountController : ControllerBase
{
    private readonly IDiscountRepository repository;

    public DiscountController(IDiscountRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet("{productName}", Name = "GetDiscount")]
    [ProducesResponseType(typeof(Coupon), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Coupon>> GetDiscount(string productName)
    {
        var coupon = await repository.GetDiscount(productName);
        return coupon is null ? NotFound() : Ok(coupon);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Coupon), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Coupon>> CreateDiscount([FromBody] Coupon coupon)
    {
        if (!await repository.CreateDiscount(coupon))
        {
            return Conflict($"A discount for '{coupon.ProductName}' already exists.");
        }

        return CreatedAtRoute("GetDiscount", new { productName = coupon.ProductName }, coupon);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateDiscount([FromBody] Coupon coupon)
    {
        return await repository.UpdateDiscount(coupon) ? Ok(true) : NotFound();
    }

    [HttpDelete("{productName}", Name = "DeleteDiscount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteDiscount(string productName)
    {
        return await repository.DeleteDiscount(productName) ? Ok(true) : NotFound();
    }
}
