namespace Shopping.WebApp.Services;

/// <summary>
/// The shopper the pages act on behalf of. Every page asks this service instead of
/// hardcoding a user name, so basket, checkout and order pages all see the same user.
/// </summary>
public interface ICurrentUser
{
    string UserName { get; }
}
