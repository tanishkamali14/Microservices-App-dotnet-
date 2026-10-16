using Shopping.WebApp.Models;

namespace Shopping.WebApp.Services;

public interface IBasketService
{
    Task<BasketModel> GetBasket(string username);
    Task<BasketModel> UpdateBasket(BasketModel model);
    Task<BasketModel> AddItem(string userName, CatalogModel product, int quantity, string color);
    Task CheckoutBasket(BasketCheckoutModel model);
}
