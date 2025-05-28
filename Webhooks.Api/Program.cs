using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Webhooks.Api.Data;
using Webhooks.Api.Extensions;
using Webhooks.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddDbContext<WebhookDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Webhooks");
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging(); // Enable for development only
});

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<WebhookDispatcher>();


builder.Services.AddHostedService<WebhookProcessor>();

builder.Services.AddSingleton(_ =>
{
var channelOptions = new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = true,
    SingleWriter = true
};
return Channel.CreateBounded<WebhookDispatch>(channelOptions);
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(op =>
    {
        op.SwaggerEndpoint("/swagger/v1/swagger.json", "Webhooks API");
    });

    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();


app.MapPost("/webhooks/subscriptions", async (
    CreateWebhookSubscriptionRequest request,
    WebhookDbContext webhookDbContext) =>
{
    var subscription = new WebhookSubscription(
        Guid.NewGuid(),
        request.EventType,
        request.WebhookUrl,
        DateTime.UtcNow);

    webhookDbContext.WebhookSubscriptions.Add(subscription);

    await webhookDbContext.SaveChangesAsync();

    return Results.Ok(subscription);
})
.WithTags("Webhook Subscriptions");

app.MapPost("/orders", async (
    WebhookDbContext webhookDbContext,
    CreateOrderRequest request,
    WebhookDispatcher webhookDispatcher) =>
{
    var order = new Order(
        Guid.NewGuid(),
        request.CustomerName,
        request.Amount,
        DateTime.UtcNow);

    webhookDbContext.Orders.Add(order);

    await webhookDbContext.SaveChangesAsync();

    // Dispatch webhook asynchronously
    await webhookDispatcher.DispatchAsync("order.created", order);
    return Results.Ok(order);
})
.WithTags("Orders");

app.MapGet("/webhooks/subscriptions", async (WebhookDbContext webhookDbContext) =>
{
    var subscriptions = await webhookDbContext.WebhookSubscriptions.ToListAsync();
    return Results.Ok(subscriptions);
});

app.Run();
