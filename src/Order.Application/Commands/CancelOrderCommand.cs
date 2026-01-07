using MediatR;

namespace Order.Application.Commands;

public record CancelOrderCommand(string OrderId) : IRequest<CancelOrderResult>;

public record CancelOrderResult(
    bool Success,
    string? Message,
    List<string>? Errors
);
