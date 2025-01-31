using Webhooks.Api.Models;

namespace Webhooks.Api.Repositories;

public class InMemoryWebhookSubscriptionRepository
{
    private readonly List<WebhookSubscription> _subscriptions =[];
    public InMemoryWebhookSubscriptionRepository()
    {
        _subscriptions.Add(new WebhookSubscription(Guid.NewGuid(), "order.created", "https://webhook.site/fab36292-d452-4002-af1e-71e0a9999ab7", DateTime.UtcNow));
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