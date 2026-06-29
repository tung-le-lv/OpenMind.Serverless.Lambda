using MediatR;
using Order.Api.Domain.Enums;
using Order.Api.Shared;

namespace Order.Api.Features.GetOrdersByCustomerAndStatus;

public record GetOrdersByCustomerAndStatusQuery(string CustomerId, OrderStatus Status)
    : IRequest<IEnumerable<OrderDto>>;
