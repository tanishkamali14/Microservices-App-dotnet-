using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shopping.WebApp.Models;
using Shopping.WebApp.Services;

namespace Shopping.WebApp.Pages;

public class CheckOutModel : PageModel
{
    private readonly IBasketService basketService;
    private readonly ICurrentUser currentUser;

    public CheckOutModel(IBasketService basketService, ICurrentUser currentUser)
    {
        this.basketService = basketService;
        this.currentUser = currentUser;
    }

    [BindProperty]
    public CheckoutForm Order { get; set; } = new();

    /// <summary>Idempotency key for this checkout attempt; resubmitting the form reuses it.</summary>
    [BindProperty]
    public Guid CheckoutId { get; set; }

    public BasketModel Cart { get; set; } = new BasketModel();

    public string UserName => currentUser.UserName;

    public async Task<IActionResult> OnGetAsync()
    {
        Cart = await basketService.GetBasket(currentUser.UserName);
        if (Cart.Items.Count == 0)
        {
            return RedirectToPage("Cart");
        }

        CheckoutId = Guid.NewGuid();
        return Page();
    }

    public async Task<IActionResult> OnPostCheckOutAsync()
    {
        Cart = await basketService.GetBasket(currentUser.UserName);
        if (Cart.Items.Count == 0)
        {
            return RedirectToPage("Cart");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var checkout = new BasketCheckoutModel
        {
            CheckoutId = CheckoutId == Guid.Empty ? Guid.NewGuid() : CheckoutId,
            Username = currentUser.UserName,
            TotalPrice = Cart.TotalPrice,
            FirstName = Order.FirstName,
            LastName = Order.LastName,
            EmailAddress = Order.EmailAddress,
            AddressLine = Order.AddressLine,
            Country = Order.Country,
            State = Order.State,
            ZipCode = Order.ZipCode,
            CardName = Order.CardName,
            CardNumber = Order.CardNumber,
            Expiration = Order.Expiration,
            CVV = Order.CVV,
            PaymentMethod = Order.PaymentMethod
        };

        await basketService.CheckoutBasket(checkout);

        return RedirectToPage("Confirmation", "OrderSubmitted");
    }
}
