using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;
using Webhooks.Api.Models;
using Webhooks.Api.Repositories;
using Webhooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<InMemoryOrderRepository>();
builder.Services.AddSingleton<InMemoryWebhookSubscriptionRepository>();

builder.Services.AddHttpClient<WebhookDispatcher>();

builder.Services.AddDbContext<WebhookDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Webhooks"));
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
}

app.UseHttpsRedirection();


app.MapPost("/webhooks/subscriptions", (
    CreateWebhookSubscriptionRequest request,
    WebhookDbContext webhookDbContext) =>
{
    var subscription = new WebhookSubscription(
        Guid.NewGuid(), 
        request.EventType,
        request.WebhookUrl, 
        DateTime.UtcNow);

    webhookDbContext.WebhookSubscriptions.Add(subscription);
    return Results.Ok(subscription);
   })
    .WithTags("Webhook Subscriptions");




app.MapGet("/orders", (WebhookDbContext webhookDbContext) 
        => Results.Ok((object?)webhookDbContext.Orders.ToList()))
    .WithTags("Orders");

app.MapPost("/orders",async (
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

        await webhookDispatcher.DispatchAsync("order.created", order);
        return Results.Ok(order);
    })
    .WithTags("Orders");




app.Run();

