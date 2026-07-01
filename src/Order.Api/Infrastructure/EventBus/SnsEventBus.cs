using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Order.Api.Domain.Events;
using Order.Api.Shared.Application.Interfaces;

namespace Order.Api.Infrastructure.EventBus;

public class SnsEventBus(IAmazonSimpleNotificationService snsClient) : IEventBus
{
    private readonly string _topicArn = Environment.GetEnvironmentVariable("ORDER_EVENTS_TOPIC_ARN") ?? string.Empty;

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

        await snsClient.PublishAsync(request, cancellationToken);
    }
}
