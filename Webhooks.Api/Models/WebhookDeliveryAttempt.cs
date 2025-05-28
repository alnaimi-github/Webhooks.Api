namespace Webhooks.Api.Models;

public sealed class WebhookDeliveryAttempt
{
    public Guid Id { get; set; }
    public Guid WebhookSubscriptionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; }
    public string Payload { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }

    public bool IsSuccess { get; set; } 
}
