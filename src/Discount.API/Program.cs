using Discount.Data;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDiscountData(builder.Configuration["DatabaseSettings:ConnectionString"]!);
builder.Services.AddHealthChecks()
    .AddCheck<DiscountDatabaseHealthCheck>("postgres", tags: [ServiceDefaultsExtensions.ReadyTag]);

var app = builder.Build();

await app.Services.MigrateDiscountDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();
