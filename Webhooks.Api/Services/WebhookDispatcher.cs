using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Channels;
using Webhooks.Api.Data;
using Webhooks.Api.Models;
using Webhooks.Api.Services;

public sealed record WebhookDispatch(string EventType, object Data);

public sealed class WebhookProcessor : BackgroundService
{
    private readonly Channel<WebhookDispatch> _webhookChannel;
    private readonly IServiceScopeFactory _scopeFactory;

    public WebhookProcessor(
        Channel<WebhookDispatch> webhookChannel,
        IServiceScopeFactory scopeFactory)
    {
        _webhookChannel = webhookChannel;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var dispatch in _webhookChannel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var webhookDispatcher = scope.ServiceProvider.GetRequiredService<WebhookDispatcher>();
            await webhookDispatcher.ProcessAsync(dispatch.EventType, dispatch.Data);
        }
    }
}

public sealed class WebhookDispatcher
{
    private readonly Channel<WebhookDispatch> _webhookChannel;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;

    public WebhookDispatcher(
        Channel<WebhookDispatch> webhookChannel,
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory)
    {
        _webhookChannel = webhookChannel;
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
    }

    public async Task DispatchAsync<T>(string eventType, T data)
        where T : notnull
    {
        var dispatch = new WebhookDispatch(eventType, data!);
        await _webhookChannel.Writer.WriteAsync(dispatch);
    }

    public async Task ProcessAsync<T>(string eventType, T data)
    {
        using var scope = _scopeFactory.CreateScope();
        var webhookDbContext = scope.ServiceProvider.GetRequiredService<WebhookDbContext>();

        var subscriptions = await webhookDbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.EventType == eventType)
            .ToListAsync();

        foreach (var subscription in subscriptions)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var payload = new WebhookPayload<T>
            {
                Id = Guid.NewGuid(),
                EventType = subscription.EventType,
                SubscriptionId = subscription.Id,
                TimeStamp = DateTime.UtcNow,
                Data = data
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            try
            {
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(subscription.WebhookUrl, payload);

                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = subscription.Id,
                    TimeStamp = DateTime.UtcNow,
                    Payload = jsonPayload,
                    StatusCode = (int)httpResponseMessage.StatusCode,
                    IsSuccess = httpResponseMessage.IsSuccessStatusCode,
                };

                webhookDbContext.WebhookDeliveryAttempts.Add(attempt);
                await webhookDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = subscription.Id,
                    TimeStamp = DateTime.UtcNow,
                    Payload = jsonPayload,
                    ErrorMessage = ex.Message,
                    IsSuccess = false,
                    StatusCode = null
                };
                webhookDbContext.WebhookDeliveryAttempts.Add(attempt);
                await webhookDbContext.SaveChangesAsync();
            }
        }
    }
}
