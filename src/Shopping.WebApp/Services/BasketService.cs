using Shopping.WebApp.Extensions;
using Shopping.WebApp.Models;

namespace Shopping.WebApp.Services;

public class BasketService : IBasketService
{
    private readonly HttpClient httpClient;

    public BasketService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task CheckoutBasket(BasketCheckoutModel model)
    {
        var response = await httpClient.PostAsJson($"/Basket/Checkout", model);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Something went wrong when calling the api.");
        }
    }

    public async Task<BasketModel> GetBasket(string userName)
    {
        var response = await httpClient.GetAsync($"/Basket/{Uri.EscapeDataString(userName)}");
        return await response.ReadContentAs<BasketModel>();
    }

    public async Task<BasketModel> AddItem(string userName, CatalogModel product, int quantity, string color)
    {
        var basket = await GetBasket(userName);
        basket.UserName = userName;

        var existing = basket.Items.FirstOrDefault(i => i.ProductId == product.Id && i.Color == color);
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            basket.Items.Add(new BasketItemModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                Color = color
            });
        }

        return await UpdateBasket(basket);
    }

    public async Task<BasketModel> UpdateBasket(BasketModel model)
    {
        var response = await httpClient.PostAsJson($"/Basket", model);
        if (response.IsSuccessStatusCode)
        {
            return await response.ReadContentAs<BasketModel>();
        }
        else
        {
            throw new Exception("Something went wrong when calling the api.");
        }
    }
}
