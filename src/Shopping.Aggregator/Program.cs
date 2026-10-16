using Shopping.Aggregator.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddHttpClient<ICatalogService, CatalogService>(e =>
    e.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"]!)
).AddStandardResilienceHandler();

builder.Services.AddHttpClient<IBasketService, BasketService>(e =>
    e.BaseAddress = new Uri(builder.Configuration["ApiSettings:BasketUrl"]!)
).AddStandardResilienceHandler();

builder.Services.AddHttpClient<IOrderService, OrderService>(e =>
    e.BaseAddress = new Uri(builder.Configuration["ApiSettings:OrderingUrl"]!)
).AddStandardResilienceHandler();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
