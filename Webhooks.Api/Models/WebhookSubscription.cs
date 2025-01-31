namespace Webhooks.Api.Models;

public sealed record WebhookSubscription(Guid Id, string EventType,string WebhookUrl,DateTime CreateOnUtc);

public sealed record CreateWebhookSubscriptionRequest(string EventType, string WebhookUrl);