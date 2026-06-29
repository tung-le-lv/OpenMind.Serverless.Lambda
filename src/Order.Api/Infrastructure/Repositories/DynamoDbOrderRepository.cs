using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Repositories;

namespace Order.Api.Infrastructure.Repositories;

public class DynamoDbOrderRepository(IAmazonDynamoDB dynamoDbClient) : IOrderRepository
{
    private readonly string _tableName = Environment.GetEnvironmentVariable("ORDERS_TABLE") ?? "Orders";

    public async Task<OrderAggregate?> GetByIdAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = orderId } }
            }
        };

        var response = await dynamoDbClient.GetItemAsync(request, cancellationToken);

        return !response.IsItemSet ? null : OrderMapper.ToOrder(response.Item);
    }

    public async Task<IEnumerable<OrderAggregate>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = "CustomerIdIndex",
            KeyConditionExpression = "customerId = :customerId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":customerId", new AttributeValue { S = customerId } }
            }
        };

        var response = await dynamoDbClient.QueryAsync(request, cancellationToken);
        return response.Items.Select(OrderMapper.ToOrder);
    }

    public async Task<IEnumerable<OrderAggregate>> GetByCustomerIdAndStatusAsync(string customerId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = "CustomerIdIndex",
            KeyConditionExpression = "customerId = :customerId",
            FilterExpression = "#status = :status",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#status", "status" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":customerId", new AttributeValue { S = customerId } },
                { ":status", new AttributeValue { S = status.ToString() } }
            }
        };

        var response = await dynamoDbClient.QueryAsync(request, cancellationToken);
        return response.Items.Select(OrderMapper.ToOrder);
    }

    public async Task<IEnumerable<OrderAggregate>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = "OrderDateIndex",
            KeyConditionExpression = "orderDate = :date",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":date", new AttributeValue { S = date.ToString("yyyy-MM-dd") } }
            },
            ScanIndexForward = false
        };

        var response = await dynamoDbClient.QueryAsync(request, cancellationToken);
        return response.Items.Select(OrderMapper.ToOrder);
    }

    public async Task<IEnumerable<OrderAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var request = new ScanRequest
        {
            TableName = _tableName
        };

        var response = await dynamoDbClient.ScanAsync(request, cancellationToken);
        return response.Items.Select(OrderMapper.ToOrder);
    }

    public async Task<OrderAggregate> AddAsync(OrderAggregate order, CancellationToken cancellationToken = default)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = OrderMapper.ToAttributeValues(order)
        };

        await dynamoDbClient.PutItemAsync(request, cancellationToken);
        return order;
    }

    public async Task<OrderAggregate> UpdateAsync(OrderAggregate order, CancellationToken cancellationToken = default)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = OrderMapper.ToAttributeValues(order)
        };

        await dynamoDbClient.PutItemAsync(request, cancellationToken);
        return order;
    }

    public async Task DeleteAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = orderId } }
            }
        };

        await dynamoDbClient.DeleteItemAsync(request, cancellationToken);
    }
    
}
