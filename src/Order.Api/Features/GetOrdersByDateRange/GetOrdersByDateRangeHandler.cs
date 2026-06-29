using MediatR;
using Order.Api.Domain.Repositories;
using Order.Api.Shared;

namespace Order.Api.Features.GetOrdersByDateRange;

public class GetOrdersByDateRangeHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersByDateRangeQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByDateAsync(request.Date, cancellationToken);
        return orders.Select(OrderMapper.ToDto);
    }
}
