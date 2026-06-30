using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AWS.Lambda.Powertools.Logging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Domain.Enums;
using Order.Api.Features.UpdateOrderStatus;

namespace Order.Api.Features.HandlePaymentProcessed;

public partial class HandlePaymentProcessedFunction(IMediator mediator)
{
    public HandlePaymentProcessedFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(LogEvent = false)]
    public async Task<SQSBatchResponse> Handler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();

        foreach (var record in sqsEvent.Records)
        {
            try
            {
                var notification = JsonSerializer.Deserialize<SnsNotification>(record.Body, JsonOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize SNS notification.");

                var envelope = JsonSerializer.Deserialize<EventEnvelope>(notification.Message, JsonOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize event envelope.");

                if (envelope.EventType != "PaymentProcessed")
                {
                    continue;
                }

                var data = envelope.Data.Deserialize<PaymentProcessedData>(JsonOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize PaymentProcessed data.");

                Logger.LogInformation($"Updating order {data.OrderId} status to PaymentConfirmed");

                var result = await mediator.Send(new UpdateOrderStatusCommand(data.OrderId, OrderStatus.PaymentConfirmed));

                if (!result.Success)
                {
                    Logger.LogWarning($"Failed to update order {data.OrderId}: {result.Message}");
                    batchItemFailures.Add(new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to process SQS record {record.MessageId}");
                batchItemFailures.Add(new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId });
            }
        }

        return new SQSBatchResponse { BatchItemFailures = batchItemFailures };
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private record SnsNotification(string Message);
    private record EventEnvelope(string EventType, JsonElement Data);
    private record PaymentProcessedData(string PaymentId, string OrderId);
}
