using MediatR;
using Order.Domain.Enums;

namespace Order.Application.Commands;

public record UpdateOrderStatusCommand(
    string OrderId,
    OrderStatus NewStatus
) : IRequest<UpdateOrderStatusResult>;

public record UpdateOrderStatusResult(
    bool Success,
    string? Message,
    List<string>? Errors
);
