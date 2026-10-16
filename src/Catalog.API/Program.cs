using Catalog.API.Data;
using Catalog.API.HealthChecks;
using Catalog.API.Repositories;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration["DatabaseSettings:ConnectionString"]));
builder.Services.AddSingleton<ICatalogContext, CatalogContext>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddHostedService<CatalogSeeder>();

builder.Services.AddHealthChecks()
    .AddCheck<MongoHealthCheck>("mongodb", tags: [ServiceDefaultsExtensions.ReadyTag]);

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
