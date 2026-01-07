using MediatR;

namespace Order.Application.Commands;

public record CreateOrderCommand(
    string CustomerId,
    List<CreateOrderItemDto> Items,
    AddressDto? ShippingAddress
) : IRequest<CreateOrderResult>;

public record CreateOrderItemDto(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

public record AddressDto(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country
);

public record CreateOrderResult(
    bool Success,
    string? OrderId,
    string? Message,
    List<string>? Errors
);
