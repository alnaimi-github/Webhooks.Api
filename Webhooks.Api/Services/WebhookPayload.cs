
namespace Webhooks.Api.Services;

internal class WebhookPayload<T>
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; }
    public T Data { get; set; } = default!;
}