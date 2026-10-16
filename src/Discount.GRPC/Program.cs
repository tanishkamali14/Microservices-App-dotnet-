using Discount.Data;
using Discount.GRPC.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDiscountData(builder.Configuration["DatabaseSettings:ConnectionString"]!);

// Besides the HTTP endpoints, expose readiness through the standard grpc.health.v1 service,
// which gRPC-aware load balancers and Kubernetes gRPC probes understand.
builder.Services.AddGrpcHealthChecks(options =>
        options.Services.Map("", check => check.Tags.Contains(ServiceDefaultsExtensions.ReadyTag)))
    .AddCheck<DiscountDatabaseHealthCheck>("postgres", tags: [ServiceDefaultsExtensions.ReadyTag]);

var app = builder.Build();

await app.Services.MigrateDiscountDatabaseAsync();

// Configure the HTTP request pipeline.
app.MapGrpcService<DiscountService>();
app.MapGrpcHealthChecksService();
app.MapDefaultEndpoints();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();
