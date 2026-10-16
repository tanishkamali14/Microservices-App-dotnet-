using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.WebApp.Models;
using Shopping.WebApp.Services;

namespace Shopping.WebApp.Pages;

public class ProductDetailModel : PageModel
{
    private readonly ICatalogService catalogService;
    private readonly IBasketService basketService;
    private readonly ICurrentUser currentUser;

    public ProductDetailModel(ICatalogService catalogService, IBasketService basketService, ICurrentUser currentUser)
    {
        this.catalogService = catalogService;
        this.basketService = basketService;
        this.currentUser = currentUser;
    }

    public CatalogModel Product { get; set; } = new();

    [BindProperty]
    public string Color { get; set; } = "Black";

    [BindProperty]
    public int Quantity { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync(string? productId)
    {
        if (productId == null)
        {
            return NotFound();
        }

        var product = await catalogService.GetCatalog(productId);
        if (product == null)
        {
            return NotFound();
        }

        Product = product;
        return Page();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(string productId)
    {
        var product = await catalogService.GetCatalog(productId);
        if (product is null)
        {
            return NotFound();
        }

        var quantity = Math.Clamp(Quantity, 1, 100);
        var color = string.IsNullOrWhiteSpace(Color) ? "Black" : Color;

        await basketService.AddItem(currentUser.UserName, product, quantity, color);
        return RedirectToPage("Cart");
    }
}
