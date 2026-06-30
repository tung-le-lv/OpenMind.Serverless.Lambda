using MediatR;
using Order.Api.Domain.Repositories;
using Order.Api.Application.Dtos;

namespace Order.Api.Features.GetOrdersByDateRange;

public class GetOrdersByDateRangeQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersByDateRangeQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByDateAsync(request.Date, cancellationToken);
        return orders.Select(OrderMapper.ToDto);
    }
}
