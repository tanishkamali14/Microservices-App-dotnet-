using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.WebApp.Models;
using Shopping.WebApp.Services;

namespace Shopping.WebApp.Pages;

public class CartModel : PageModel
{
    private readonly IBasketService basketService;
    private readonly ICurrentUser currentUser;

    public CartModel(IBasketService basketService, ICurrentUser currentUser)
    {
        this.basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
        this.currentUser = currentUser;
    }

    public BasketModel Cart { get; set; } = new BasketModel();

    public async Task<IActionResult> OnGetAsync()
    {
        Cart = await basketService.GetBasket(currentUser.UserName);

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveFromCartAsync(string productId)
    {
        var basket = await basketService.GetBasket(currentUser.UserName);

        var item = basket.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item is not null)
        {
            basket.Items.Remove(item);
            await basketService.UpdateBasket(basket);
        }

        return RedirectToPage();
    }
}
