using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// Lives in the hosting namespace so every Program.cs can call it without an extra using.
namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Telemetry and health endpoints shared by every service, so each Program.cs only adds
/// what is specific to that service.
/// </summary>
public static class ServiceDefaultsExtensions
{
    /// <summary>Checks tagged "live" answer "is the process running?" (restart it if not).</summary>
    public const string LiveTag = "live";

    /// <summary>Checks tagged "ready" answer "can it reach its own databases and broker?" (stop sending traffic if not).</summary>
    public const string ReadyTag = "ready";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), [LiveTag]);

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("MassTransit"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                    // Probes hit these endpoints every few seconds; tracing them would bury real traffic.
                    options.Filter = context => !context.Request.Path.StartsWithSegments("/health"))
                .AddHttpClientInstrumentation() // also covers gRPC calls, which run over HttpClient
                .AddSource("MassTransit")       // spans for publishing and consuming RabbitMQ messages
                .AddSource("Npgsql"));          // spans for PostgreSQL commands

        // Export logs, traces and metrics only when a collector is configured.
        // docker-compose points this at the Aspire dashboard; locally nothing is exported.
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static IEndpointRouteBuilder MapDefaultEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // ShortCircuit() answers probes straight from the routing middleware, so they skip the rest
        // of the pipeline. That is also what lets the gateway serve them in front of Ocelot, which
        // otherwise handles every request.
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(LiveTag)
        }).ShortCircuit();

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(ReadyTag),
            ResponseWriter = WriteJsonAsync
        }).ShortCircuit();

        return endpoints;
    }

    private static Task WriteJsonAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        // Status and timings only: exception details stay in the logs instead of a public response.
        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds)
                })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
