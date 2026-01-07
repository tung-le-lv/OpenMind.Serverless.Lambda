using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;
using System.Text.Json;

namespace Order.Infrastructure.Repositories;

public class DynamoDbOrderRepository : IOrderRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;

    public DynamoDbOrderRepository(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = Environment.GetEnvironmentVariable("ORDERS_TABLE") ?? "Orders";
    }

    public async Task<OrderEntity?> GetByIdAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = orderId } }
            }
        };

        var response = await _dynamoDbClient.GetItemAsync(request, cancellationToken);

        if (!response.IsItemSet)
            return null;

        return MapToOrder(response.Item);
    }

    public async Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
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

        var response = await _dynamoDbClient.QueryAsync(request, cancellationToken);
        return response.Items.Select(MapToOrder);
    }

    public async Task<IEnumerable<OrderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var request = new ScanRequest
        {
            TableName = _tableName
        };

        var response = await _dynamoDbClient.ScanAsync(request, cancellationToken);
        return response.Items.Select(MapToOrder);
    }

    public async Task<OrderEntity> AddAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        var item = MapToAttributeValues(order);

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDbClient.PutItemAsync(request, cancellationToken);
        return order;
    }

    public async Task<OrderEntity> UpdateAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        var item = MapToAttributeValues(order);

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDbClient.PutItemAsync(request, cancellationToken);
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

        await _dynamoDbClient.DeleteItemAsync(request, cancellationToken);
    }

    private static Dictionary<string, AttributeValue> MapToAttributeValues(OrderEntity order)
    {
        var items = order.Items.Select(i => new OrderItemData
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice.Amount
        }).ToList();

        var item = new Dictionary<string, AttributeValue>
        {
            { "id", new AttributeValue { S = order.Id } },
            { "customerId", new AttributeValue { S = order.CustomerId } },
            { "totalAmount", new AttributeValue { N = order.TotalAmount.Amount.ToString() } },
            { "currency", new AttributeValue { S = order.TotalAmount.Currency } },
            { "status", new AttributeValue { S = order.Status.ToString() } },
            { "createdAt", new AttributeValue { S = order.CreatedAt.ToString("O") } },
            { "updatedAt", new AttributeValue { S = order.UpdatedAt.ToString("O") } },
            { "items", new AttributeValue { S = JsonSerializer.Serialize(items) } }
        };

        if (order.ShippingAddress != null)
        {
            var addressData = new AddressData
            {
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                ZipCode = order.ShippingAddress.ZipCode,
                Country = order.ShippingAddress.Country
            };
            item["shippingAddress"] = new AttributeValue { S = JsonSerializer.Serialize(addressData) };
        }

        return item;
    }

    private static OrderEntity MapToOrder(Dictionary<string, AttributeValue> item)
    {
        var items = new List<OrderItem>();
        if (item.TryGetValue("items", out var itemsAttr))
        {
            var itemsData = JsonSerializer.Deserialize<List<OrderItemData>>(itemsAttr.S) ?? [];
            items = itemsData.Select(i => OrderItem.Reconstitute(
                i.ProductId, i.ProductName, i.Quantity, i.UnitPrice
            )).ToList();
        }

        Address? shippingAddress = null;
        if (item.TryGetValue("shippingAddress", out var addressAttr))
        {
            var addressData = JsonSerializer.Deserialize<AddressData>(addressAttr.S);
            if (addressData != null)
            {
                shippingAddress = Address.Create(
                    addressData.Street,
                    addressData.City,
                    addressData.State,
                    addressData.ZipCode,
                    addressData.Country
                );
            }
        }

        return OrderEntity.Reconstitute(
            id: item["id"].S,
            customerId: item["customerId"].S,
            items: items,
            totalAmount: decimal.Parse(item["totalAmount"].N),
            status: Enum.Parse<OrderStatus>(item["status"].S),
            shippingAddress: shippingAddress,
            createdAt: DateTime.Parse(item["createdAt"].S),
            updatedAt: DateTime.Parse(item["updatedAt"].S)
        );
    }

    private class OrderItemData
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    private class AddressData
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
