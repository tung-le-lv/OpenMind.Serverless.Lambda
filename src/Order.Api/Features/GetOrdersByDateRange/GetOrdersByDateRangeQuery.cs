using MediatR;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrdersByDateRange;

public record GetOrdersByDateRangeQuery(DateOnly Date) : IRequest<IEnumerable<OrderDto>>;
