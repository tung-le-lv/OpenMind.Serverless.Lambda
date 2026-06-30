using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Repositories;

namespace Payment.Api.Infrastructure.Repositories;

public class DynamoDbPaymentRepository(IAmazonDynamoDB dynamoDbClient) : IPaymentRepository
{
    private readonly string _tableName = Environment.GetEnvironmentVariable("PAYMENTS_TABLE") ?? "Payments";

    public async Task<PaymentAggregate?> GetByIdAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        var response = await dynamoDbClient.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = paymentId } } }
        }, cancellationToken);

        return !response.IsItemSet ? null : ToPayment(response.Item);
    }

    public async Task<PaymentAggregate?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var response = await dynamoDbClient.QueryAsync(new QueryRequest
        {
            TableName = _tableName,
            IndexName = "OrderIdIndex",
            KeyConditionExpression = "orderId = :orderId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":orderId", new AttributeValue { S = orderId } }
            },
            Limit = 1
        }, cancellationToken);

        return response.Items.Count == 0 ? null : ToPayment(response.Items[0]);
    }

    public async Task<PaymentAggregate> AddAsync(PaymentAggregate payment, CancellationToken cancellationToken = default)
    {
        await dynamoDbClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeValues(payment)
        }, cancellationToken);

        return payment;
    }

    private static Dictionary<string, AttributeValue> ToAttributeValues(PaymentAggregate payment) =>
        new()
        {
            { "id", new AttributeValue { S = payment.Id } },
            { "orderId", new AttributeValue { S = payment.OrderId } },
            { "customerId", new AttributeValue { S = payment.CustomerId } },
            { "amount", new AttributeValue { N = payment.Amount.ToString() } },
            { "status", new AttributeValue { S = payment.Status.ToString() } },
            { "failureReason", new AttributeValue { S = payment.FailureReason ?? string.Empty } },
            { "createdAt", new AttributeValue { S = payment.CreatedAt.ToString("O") } },
            { "processedAt", new AttributeValue { S = payment.ProcessedAt.ToString("O") } }
        };

    private static PaymentAggregate ToPayment(Dictionary<string, AttributeValue> item) =>
        PaymentAggregate.Reconstitute(
            id: item["id"].S,
            orderId: item["orderId"].S,
            customerId: item["customerId"].S,
            amount: decimal.Parse(item["amount"].N),
            status: Enum.Parse<PaymentStatus>(item["status"].S),
            failureReason: item.TryGetValue("failureReason", out var fr) && !string.IsNullOrEmpty(fr.S) ? fr.S : null,
            createdAt: DateTime.Parse(item["createdAt"].S),
            processedAt: DateTime.Parse(item["processedAt"].S)
        );
}
