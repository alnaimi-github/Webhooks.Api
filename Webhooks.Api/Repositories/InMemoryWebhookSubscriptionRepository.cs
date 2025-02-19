using Webhooks.Api.Models;

namespace Webhooks.Api.Repositories;

public class InMemoryWebhookSubscriptionRepository
{
    private readonly List<WebhookSubscription> _subscriptions =[];
    public InMemoryWebhookSubscriptionRepository()
    {
        _subscriptions.Add(new WebhookSubscription(Guid.NewGuid(), "order.created", "http://localhost:3000/api/webhook", DateTime.UtcNow));
        _subscriptions.Add(new WebhookSubscription(Guid.NewGuid(), "OrderUpdated", "https://webhook.site/2", DateTime.UtcNow));
    }

    public void AddSubscription(WebhookSubscription subscription)
    {
        _subscriptions.Add(subscription);
    }

    public IReadOnlyList<WebhookSubscription> GetByEventType(string eventType)
    {
        return _subscriptions.Where(s => s.EventType == eventType).ToList();
    }
}