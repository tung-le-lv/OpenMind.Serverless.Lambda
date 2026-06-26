using System.Text.Json;
using Amazon.DynamoDBv2.Model;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Infrastructure.Repositories;

internal static class OrderMapper
{
    internal static OrderAggregate ToOrder(Dictionary<string, AttributeValue> item)
    {
        var itemsData = JsonSerializer.Deserialize<List<OrderItemData>>(item["items"].S) ?? [];
        var orderItems = itemsData.Select(i => OrderItem.Reconstitute(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList();

        Address? shippingAddress = null;
        if (item.TryGetValue("shippingAddress", out var addrAttr))
        {
            var addr = JsonSerializer.Deserialize<AddressData>(addrAttr.S)!;
            shippingAddress = Address.Create(addr.Street, addr.City, addr.State, addr.ZipCode, addr.Country);
        }

        return OrderAggregate.Reconstitute(
            id: item["id"].S,
            customerId: item["customerId"].S,
            items: orderItems,
            totalAmount: decimal.Parse(item["totalAmount"].N),
            status: Enum.Parse<OrderStatus>(item["status"].S),
            shippingAddress: shippingAddress,
            createdAt: DateTime.Parse(item["createdAt"].S),
            updatedAt: DateTime.Parse(item["updatedAt"].S)
        );
    }

    internal static Dictionary<string, AttributeValue> ToAttributeValues(OrderAggregate order)
    {
        var itemsData = order.Items.Select(i => new OrderItemData
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
            { "items", new AttributeValue { S = JsonSerializer.Serialize(itemsData) } }
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

}
