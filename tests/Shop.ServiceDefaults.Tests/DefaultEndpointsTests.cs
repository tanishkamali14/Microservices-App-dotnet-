using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shop.ServiceDefaults.Tests;

/// <summary>
/// Starts a real Kestrel server on a free port with the shared defaults, one readiness check that
/// always fails, and a terminal middleware standing in for Ocelot.
/// </summary>
public class DefaultEndpointsTests : IAsyncLifetime
{
    private const string SecretDetail = "Password=hunter2";

    private WebApplication app = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.AddServiceDefaults();
        builder.Services.AddHealthChecks()
            .AddCheck("database",
                () => HealthCheckResult.Unhealthy("Database is down.", new InvalidOperationException(SecretDetail)),
                [ServiceDefaultsExtensions.ReadyTag]);

        app = builder.Build();
        app.MapDefaultEndpoints();
        app.Run(context =>
        {
            context.Response.StatusCode = StatusCodes.Status418ImATeapot;
            return Task.CompletedTask;
        });

        await app.StartAsync();
        client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    [Fact]
    public async Task Liveness_stays_healthy_when_a_dependency_is_down()
    {
        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Readiness_returns_503_and_names_the_failing_check()
    {
        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Unhealthy", json.RootElement.GetProperty("status").GetString());

        var checks = json.RootElement.GetProperty("checks");
        Assert.Equal("Unhealthy", checks.GetProperty("database").GetProperty("status").GetString());
        Assert.False(checks.TryGetProperty("self", out _), "Liveness-only checks should not affect readiness.");
    }

    [Fact]
    public async Task Readiness_does_not_leak_exception_details()
    {
        var response = await client.GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(SecretDetail, body);
        Assert.DoesNotContain("Database is down.", body);
    }

    [Fact]
    public async Task Health_endpoints_are_answered_before_terminal_middleware()
    {
        // The gateway relies on this: Ocelot handles every request that reaches it.
        var health = await client.GetAsync("/health/live");
        var other = await client.GetAsync("/Catalog");

        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Equal(StatusCodes.Status418ImATeapot, (int)other.StatusCode);
    }
}
