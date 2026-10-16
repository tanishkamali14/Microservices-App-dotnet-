using Basket.API.Entities;
using Basket.API.Mapping;
using Basket.API.Repositories;
using Basket.API.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BasketController : ControllerBase
{
    private readonly ILogger<BasketController> logger;
    private readonly IBasketRepository repository;
    private readonly BasketPricingService pricing;
    private readonly IPublishEndpoint publishEndpoint;

    public BasketController(ILogger<BasketController> logger, IBasketRepository repository, BasketPricingService pricing, IPublishEndpoint publishEndpoint)
    {
        this.logger = logger;
        this.repository = repository;
        this.pricing = pricing;
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket, CancellationToken cancellationToken)
    {
        try
        {
            // Re-price from the catalog on every save: the old code subtracted the discount from
            // whatever price the client sent, so each save applied the discount again.
            await pricing.PriceAsync(basket, cancellationToken);
        }
        catch (InvalidBasketException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid basket", detail: ex.Message);
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
    public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasket(basketCheckout.Username);
        if (basket == null || basket.Items.Count == 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Basket is empty");
        }

        var checkoutId = basketCheckout.CheckoutId ?? Guid.NewGuid();
        var eventMessage = basketCheckout.ToCheckoutEvent(basket.TotalPrice);

        // Publish first, then delete: if the delete fails the shopper can retry with the same
        // CheckoutId, and Ordering's inbox drops the duplicate message instead of creating a second order.
        await publishEndpoint.Publish(eventMessage, context => context.MessageId = checkoutId, cancellationToken);

        await repository.DeleteBasket(basket.Username);

        logger.LogInformation("Checkout {CheckoutId} accepted for {Username}", checkoutId, basket.Username);

        return Accepted();
    }
}
