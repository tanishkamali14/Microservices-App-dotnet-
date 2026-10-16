using Basket.API.GrpcService;
using Basket.API.Repositories;
using Basket.API.Services;
using Discount.GRPC.Protos;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
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
        opt.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!))
    .ConfigureChannel(channel =>
    {
        // gRPC's built-in retry policy: retry calls that fail while Discount is briefly unavailable.
        channel.ServiceConfig = new ServiceConfig
        {
            MethodConfigs =
            {
                new MethodConfig
                {
                    Names = { MethodName.Default },
                    RetryPolicy = new RetryPolicy
                    {
                        MaxAttempts = 4,
                        InitialBackoff = TimeSpan.FromMilliseconds(200),
                        MaxBackoff = TimeSpan.FromSeconds(2),
                        BackoffMultiplier = 2,
                        RetryableStatusCodes = { StatusCode.Unavailable }
                    }
                }
            }
        };
    });
builder.Services.AddScoped<IDiscountClient, DiscountGrpcService>();
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"]!))
    .AddStandardResilienceHandler(); // retries, timeouts and a circuit breaker for catalog lookups
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
