using System.Text;
using System.Text.Json;
using Webhooks.Api.Repositories;

namespace Webhooks.Api.Services;

public sealed record WebhookDispatcher(
    HttpClient Http,
    InMemoryWebhookSubscriptionRepository SubscriptionRepository)
{
    public async Task DispatchAsync(string eventType, object payload)
    {
        var subscriptions = SubscriptionRepository.GetByEventType(eventType);
        foreach (var subscription in subscriptions)
        {

            var request = new
            {
                Id = Guid.NewGuid(),
                subscription.EventType,
                SubscriptionId = subscription.Id,
                TimeStamp = DateTime.UtcNow,
                Data = payload
            };

            await Http.PostAsJsonAsync(subscription.WebhookUrl, request);
        }
    }

}