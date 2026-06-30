using MediatR;

namespace Order.Api.Features.PlaceOrder;

public record PlaceOrderCommand(string OrderId) : IRequest<PlaceOrderResult>;

public record PlaceOrderResult(bool Success, string? Message, List<string>? Errors);
