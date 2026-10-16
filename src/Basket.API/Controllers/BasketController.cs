using Basket.API.Entities;
using Basket.API.GrpcService;
using Basket.API.Mapping;
using Basket.API.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BasketController : ControllerBase
{
    private readonly ILogger<BasketController> logger;
    private readonly IBasketRepository repository;
    private readonly DiscountGrpcService discountGrpcService;
    private readonly IPublishEndpoint publishEndpoint;

    public BasketController(ILogger<BasketController> logger, IBasketRepository repository, DiscountGrpcService discountGrpcService, IPublishEndpoint publishEndpoint)
    {
        this.logger = logger;
        this.repository = repository;
        this.discountGrpcService = discountGrpcService;
        this.publishEndpoint = publishEndpoint;
    }

    [HttpGet("{username}", Name = "GetBasket")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ShoppingCart>> GetBasket(string username)
    {
        var basket = await repository.GetBasket(username);
        return Ok(basket ?? new ShoppingCart(username));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
    {
        foreach (var item in basket.Items)
        {
            var coupon = await discountGrpcService.GetDiscount(item.ProductName);
            item.Price -= coupon.Amount;
        }

        return Ok(await repository.UpdateBasket(basket));
    }

    [HttpDelete("{username}", Name = "DeleteBasket")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteBasket(string username)
    {
        await repository.DeleteBasket(username);
        return Ok();
    }

    [Route("[action]")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
    {
        var basket = await repository.GetBasket(basketCheckout.Username);
        if (basket == null)
        {
            return BadRequest();
        }

        var eventMessage = basketCheckout.ToCheckoutEvent(basket.TotalPrice);
        await publishEndpoint.Publish(eventMessage);

        await repository.DeleteBasket(basket.Username);

        logger.LogInformation("Checkout accepted for {Username}", basket.Username);

        return Accepted();
    }
}
