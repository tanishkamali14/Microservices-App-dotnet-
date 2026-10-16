namespace Basket.API.Services;

public class InvalidBasketException : Exception
{
    public InvalidBasketException(string message) : base(message)
    {
    }
}
