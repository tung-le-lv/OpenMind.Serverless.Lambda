using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public record GetOrdersByCustomerQuery(string CustomerId) : IRequest<IEnumerable<OrderDto>>;
