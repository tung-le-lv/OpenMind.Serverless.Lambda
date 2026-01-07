using MediatR;

namespace Order.Application.Commands;

public record DeleteOrderCommand(string OrderId) : IRequest<DeleteOrderResult>;

public record DeleteOrderResult(
    bool Success,
    string? Message
);
