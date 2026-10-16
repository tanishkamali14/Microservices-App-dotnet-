using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.WebApp.Models;
using Shopping.WebApp.Services;

namespace Shopping.WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ICatalogService catalogService;
    private readonly IBasketService basketService;
    private readonly ICurrentUser currentUser;

    public IndexModel(ICatalogService catalogService, IBasketService basketService, ICurrentUser currentUser)
    {
        this.catalogService = catalogService;
        this.basketService = basketService;
        this.currentUser = currentUser;
    }

    public IEnumerable<CatalogModel> ProductList { get; set; } = new List<CatalogModel>();

    public async Task<IActionResult> OnGetAsync()
    {
        ProductList = await catalogService.GetCatalog();
        return Page();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(string productId)
    {
        var product = await catalogService.GetCatalog(productId);
        if (product is null)
        {
            return NotFound();
        }

        await basketService.AddItem(currentUser.UserName, product, quantity: 1, color: "Black");
        return RedirectToPage("Cart");
    }
}
