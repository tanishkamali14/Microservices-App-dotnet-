using EventBus.Messages.Common;
using MassTransit;
using Ordering.API.EventBusConsumer;
using Ordering.API.Extensions;
using Ordering.API.Infrastructure;
using Ordering.Application;
using Ordering.Application.Common.Exceptions;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<BasketCheckoutConsumer>();

    // Transactional inbox/outbox backed by the Ordering database. The order and the record that
    // the message was consumed commit in one transaction, so redeliveries are detected by MessageId.
    config.AddEntityFrameworkOutbox<OrderContext>(outbox =>
    {
        outbox.UseSqlServer();
        outbox.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
    });

    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);

        cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, endpoint =>
        {
            // Retry transient failures (e.g. SQL Server failover) before giving up and moving the
            // message to the _error queue. Invalid messages will never succeed, so don't retry them.
            endpoint.UseMessageRetry(retry =>
            {
                retry.Intervals(100, 500, 1_000, 5_000, 15_000);
                retry.Ignore<ValidationException>();
            });
            endpoint.UseEntityFrameworkOutbox<OrderContext>(ctx);
            endpoint.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
        });
    });
});

var app = builder.Build();

await app.Services.MigrateDatabaseAsync<OrderContext>(async (context, services) =>
{
    var logger = services.GetRequiredService<ILogger<OrderContextSeed>>();
    await OrderContextSeed.SeedAsync(context, logger);
});

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
