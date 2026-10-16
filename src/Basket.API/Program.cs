using Basket.API.GrpcService;
using Basket.API.Repositories;
using Basket.API.Services;
using Discount.GRPC.Protos;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration["CacheSettings:ConnectionString"];
});

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(opt =>
    opt.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!)
);
builder.Services.AddScoped<IDiscountClient, DiscountGrpcService>();
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"]!));
builder.Services.AddScoped<BasketPricingService>();

builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
