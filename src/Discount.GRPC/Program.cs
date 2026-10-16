using Discount.Data;
using Discount.GRPC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDiscountData(builder.Configuration["DatabaseSettings:ConnectionString"]!);

var app = builder.Build();

await app.Services.MigrateDiscountDatabaseAsync();

// Configure the HTTP request pipeline.
app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();
