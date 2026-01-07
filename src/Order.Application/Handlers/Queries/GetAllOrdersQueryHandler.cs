using MediatR;
using Order.Application.DTOs;
using Order.Application.Mappers;
using Order.Application.Queries;
using Order.Domain.Repositories;

namespace Order.Application.Handlers.Queries;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(OrderMapper.ToDto);
    }
}
