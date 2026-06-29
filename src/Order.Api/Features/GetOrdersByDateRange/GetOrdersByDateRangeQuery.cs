using MediatR;
using Order.Api.Shared;

namespace Order.Api.Features.GetOrdersByDateRange;

public record GetOrdersByDateRangeQuery(DateOnly Date) : IRequest<IEnumerable<OrderDto>>;
