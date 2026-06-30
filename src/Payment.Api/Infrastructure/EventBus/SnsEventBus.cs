using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Payment.Api.Application.Interfaces;
using Payment.Api.Domain.Events;

namespace Payment.Api.Infrastructure.EventBus;

public class SnsEventBus(IAmazonSimpleNotificationService snsClient) : IEventBus
{
    private readonly string _topicArn = Environment.GetEnvironmentVariable("PAYMENT_EVENTS_TOPIC_ARN") ?? string.Empty;

    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        if (string.IsNullOrEmpty(_topicArn))
        {
            return;
        }

        var message = new
        {
            EventId = domainEvent.EventId,
            EventType = domainEvent.EventType,
            OccurredAt = domainEvent.OccurredAt,
            Data = (object)domainEvent
        };

        await snsClient.PublishAsync(new PublishRequest
        {
            TopicArn = _topicArn,
            Message = JsonSerializer.Serialize(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "EventType",
                    new MessageAttributeValue { DataType = "String", StringValue = domainEvent.EventType }
                }
            }
        }, cancellationToken);
    }
}
