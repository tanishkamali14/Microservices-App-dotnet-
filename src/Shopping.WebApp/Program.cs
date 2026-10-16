using Microsoft.Extensions.Http.Resilience;
using Shopping.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddHttpClient<ICatalogService, CatalogService>(opt =>
{
    opt.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
}).AddStandardResilienceHandler();
builder.Services.AddHttpClient<IBasketService, BasketService>(opt =>
{
    opt.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
}).AddStandardResilienceHandler(options =>
{
    // Never blindly replay POSTs such as checkout; reads are still retried.
    options.Retry.DisableForUnsafeHttpMethods();
});
builder.Services.AddHttpClient<IOrderService, OrderService>(opt =>
{
    opt.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
}).AddStandardResilienceHandler();

builder.Services.AddScoped<ICurrentUser, ConfiguredCurrentUser>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapDefaultEndpoints();

app.Run();
