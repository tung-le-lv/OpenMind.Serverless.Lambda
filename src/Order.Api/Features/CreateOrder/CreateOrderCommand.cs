using MediatR;

namespace Order.Api.Features.CreateOrder;

public record CreateOrderCommand(
    string CustomerId,
    List<CreateOrderItemDto> Items
) : IRequest<CreateOrderResult>;

public record CreateOrderItemDto(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

public record CreateOrderResult(
    bool Success,
    string? OrderId,
    string? Message,
    List<string>? Errors
);
