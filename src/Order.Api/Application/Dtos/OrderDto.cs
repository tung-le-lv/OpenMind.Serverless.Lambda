using Order.Api.Domain.Enums;

namespace Order.Api.Application.Dtos;

public record OrderDto(
    string Id,
    string CustomerId,
    List<OrderItemDto> Items,
    decimal TotalAmount,
    string Currency,
    OrderStatus Status,
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
