using Order.Application.DTOs;
using Order.Domain.Entities;

namespace Order.Application.Mappers;

public static class OrderMapper
{
    public static OrderDto ToDto(OrderEntity order)
    {
        return new OrderDto(
            Id: order.Id,
            CustomerId: order.CustomerId,
            Items: order.Items.Select(ToItemDto).ToList(),
            TotalAmount: order.TotalAmount.Amount,
            Currency: order.TotalAmount.Currency,
            Status: order.Status,
            ShippingAddress: order.ShippingAddress != null ? ToAddressDto(order.ShippingAddress) : null,
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt
        );
    }

    public static OrderItemDto ToItemDto(OrderItem item)
    {
        return new OrderItemDto(
            ProductId: item.ProductId,
            ProductName: item.ProductName,
            Quantity: item.Quantity,
            UnitPrice: item.UnitPrice.Amount,
            Subtotal: item.Subtotal.Amount
        );
    }

    public static AddressDto ToAddressDto(Domain.ValueObjects.Address address)
    {
        return new AddressDto(
            Street: address.Street,
            City: address.City,
            State: address.State,
            ZipCode: address.ZipCode,
            Country: address.Country
        );
    }
}
