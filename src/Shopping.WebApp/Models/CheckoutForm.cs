using System.ComponentModel.DataAnnotations;

namespace Shopping.WebApp.Models;

/// <summary>
/// Fields the shopper fills in on the checkout page. The user name and the total
/// are never taken from the form: they come from the signed-in user and the basket.
/// </summary>
public class CheckoutForm
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress]
    public string EmailAddress { get; set; } = "";

    [Required, StringLength(200)]
    public string AddressLine { get; set; } = "";

    [Required]
    public string Country { get; set; } = "";

    [Required]
    public string State { get; set; } = "";

    [Required, RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Enter a 5-digit ZIP code.")]
    public string ZipCode { get; set; } = "";

    [Range(1, 3, ErrorMessage = "Choose a payment method.")]
    public int PaymentMethod { get; set; } = 1;

    [Required, StringLength(100)]
    public string CardName { get; set; } = "";

    [Required, CreditCard]
    public string CardNumber { get; set; } = "";

    [Required, RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Use MM/YY.")]
    public string Expiration { get; set; } = "";

    [Required, RegularExpression(@"^\d{3,4}$", ErrorMessage = "Enter the 3 or 4 digit security code.")]
    public string CVV { get; set; } = "";
}
