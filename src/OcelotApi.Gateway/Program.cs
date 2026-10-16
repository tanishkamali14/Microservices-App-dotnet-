using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Routes live in ocelot.{Environment}.json: "Development" targets the Docker network,
// "Local" targets services started with `dotnet run` on localhost.
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

await app.UseOcelot();
await app.RunAsync();
