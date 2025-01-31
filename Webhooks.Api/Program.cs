using Microsoft.Extensions.DependencyInjection;
using Webhooks.Api.Models;
using Webhooks.Api.Repositories;
using Webhooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<InMemoryOrderRepository>();
builder.Services.AddSingleton<InMemoryWebhookSubscriptionRepository>();

builder.Services.AddHttpClient<WebhookDispatcher>();

var app = builder.Build();

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
    InMemoryWebhookSubscriptionRepository webhookSubscriptionRepository) =>
{
    var subscription = new WebhookSubscription(Guid.NewGuid(), request.EventType, request.WebhookUrl, DateTime.UtcNow);
    webhookSubscriptionRepository.AddSubscription(subscription);
    return Results.Ok(subscription);
   })
    .WithTags("Webhook Subscriptions");




app.MapGet("/orders", (InMemoryOrderRepository orderRepository) 
        => Results.Ok((object?)orderRepository.GetAll()))
    .WithTags("Orders");

app.MapPost("/orders",async (
        InMemoryOrderRepository orderRepository,
        CreateOrderRequest request,
        WebhookDispatcher webhookDispatcher) =>
    {
        var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);
        orderRepository.AddOrder(order);

        await webhookDispatcher.DispatchAsync("order.created", order);
        return Results.Ok(order);
    })
    .WithTags("Orders");




app.Run();

