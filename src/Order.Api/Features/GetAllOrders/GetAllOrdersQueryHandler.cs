using MediatR;
using Order.Api.Domain.Repositories;
using Order.Api.Application.Dtos;

namespace Order.Api.Features.GetAllOrders;

public class GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(OrderMapper.ToDto);
    }
}
