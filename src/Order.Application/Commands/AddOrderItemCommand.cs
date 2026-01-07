using MediatR;

namespace Order.Application.Commands;

public record AddOrderItemCommand(
    string OrderId,
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
) : IRequest<AddOrderItemResult>;

public record AddOrderItemResult(
    bool Success,
    string? Message,
    List<string>? Errors
);
