using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.WebApp.Models;
using Shopping.WebApp.Services;

namespace Shopping.WebApp.Pages;

public class ProductModel : PageModel
{
    private readonly ICatalogService catalogService;
    private readonly IBasketService basketService;
    private readonly ICurrentUser currentUser;

    public ProductModel(ICatalogService catalogService, IBasketService basketService, ICurrentUser currentUser)
    {
        this.catalogService = catalogService;
        this.basketService = basketService;
        this.currentUser = currentUser;
    }

    public IEnumerable<string> CategoryList { get; set; } = new List<string>();
    public IEnumerable<CatalogModel> ProductList { get; set; } = new List<CatalogModel>();

    [BindProperty(SupportsGet = true)]
    public string? SelectedCategory { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var productList = (await catalogService.GetCatalog()).ToList();
        CategoryList = productList.Select(e => e.Category).Distinct().OrderBy(c => c);

        ProductList = string.IsNullOrWhiteSpace(SelectedCategory)
            ? productList
            : productList.Where(e => e.Category == SelectedCategory).ToList();

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
