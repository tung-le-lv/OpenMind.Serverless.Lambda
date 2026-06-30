using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AWS.Lambda.Powertools.Logging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Payment.Api.Features.ProcessPayment;

public partial class ProcessPaymentFunction(IMediator mediator)
{
    public ProcessPaymentFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

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

                if (envelope.EventType != "OrderPlaced")
                {
                    continue;
                }

                var data = envelope.Data.Deserialize<OrderPlacedData>(JsonOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize OrderPlaced data.");

                Logger.LogInformation($"Processing payment for order {data.OrderId}, customer {data.CustomerId}, amount {data.TotalAmount}");

                var result = await mediator.Send(new ProcessPaymentCommand(data.OrderId, data.CustomerId, data.TotalAmount));

                if (!result.Success)
                {
                    Logger.LogWarning($"Payment failed for order {data.OrderId}: {result.Message}");
                }

                Logger.LogInformation($"Payment processed for order {data.OrderId}, success: {result.Success}");
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
    private record OrderPlacedData(string OrderId, string CustomerId, decimal TotalAmount);
}
