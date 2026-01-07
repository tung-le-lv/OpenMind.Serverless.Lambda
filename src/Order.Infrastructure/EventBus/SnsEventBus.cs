using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Order.Application.Interfaces;
using Order.Domain.Events;
using System.Text.Json;

namespace Order.Infrastructure.EventBus;

public class SnsEventBus : IEventBus
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly string _topicArn;

    public SnsEventBus(IAmazonSimpleNotificationService snsClient)
    {
        _snsClient = snsClient;
        _topicArn = Environment.GetEnvironmentVariable("ORDER_EVENTS_TOPIC_ARN") ?? string.Empty;
    }

    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        if (string.IsNullOrEmpty(_topicArn))
        {
            // Log warning - topic not configured
            return;
        }

        var message = new
        {
            EventId = domainEvent.EventId,
            EventType = domainEvent.EventType,
            OccurredAt = domainEvent.OccurredAt,
            Data = domainEvent
        };

        var request = new PublishRequest
        {
            TopicArn = _topicArn,
            Message = JsonSerializer.Serialize(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "EventType",
                    new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = domainEvent.EventType
                    }
                }
            }
        };

        await _snsClient.PublishAsync(request, cancellationToken);
    }
}
