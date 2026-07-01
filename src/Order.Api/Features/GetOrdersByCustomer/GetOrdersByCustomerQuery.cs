using MediatR;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(string CustomerId) : IRequest<IEnumerable<OrderDto>>;
