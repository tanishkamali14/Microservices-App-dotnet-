namespace Shopping.WebApp.Services;

/// <summary>
/// Until the web app has a login, every request acts as a single demo shopper
/// configured under <c>ShopSettings:DemoUserName</c>.
/// </summary>
public class ConfiguredCurrentUser : ICurrentUser
{
    public ConfiguredCurrentUser(IConfiguration configuration)
    {
        UserName = configuration["ShopSettings:DemoUserName"] ?? "swn";
    }

    public string UserName { get; }
}
