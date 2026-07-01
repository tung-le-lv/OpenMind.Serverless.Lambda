using MediatR;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrder;

public record GetOrderQuery(string OrderId) : IRequest<OrderDto?>;
