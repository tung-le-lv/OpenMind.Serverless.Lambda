using Order.Domain.Enums;

namespace Order.Application.DTOs;

public record OrderDto(
    string Id,
    string CustomerId,
    List<OrderItemDto> Items,
    decimal TotalAmount,
    string Currency,
    OrderStatus Status,
    AddressDto? ShippingAddress,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record OrderItemDto(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);

public record AddressDto(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country
);
