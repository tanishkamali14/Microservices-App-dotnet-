using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Routes live in ocelot.{Environment}.json: "Development" targets the Docker network,
// "Local" targets services started with `dotnet run` on localhost.
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Health endpoints are short-circuited by routing, so they are answered before Ocelot,
// which would otherwise treat /health/* as an unknown route and return 404.
app.MapDefaultEndpoints();

await app.UseOcelot();
await app.RunAsync();
