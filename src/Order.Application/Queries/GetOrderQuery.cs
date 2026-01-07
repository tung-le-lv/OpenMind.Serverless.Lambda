using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public record GetOrderQuery(string OrderId) : IRequest<OrderDto?>;
