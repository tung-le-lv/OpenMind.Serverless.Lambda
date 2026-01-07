using MediatR;
using Order.Application.DTOs;
using Order.Application.Mappers;
using Order.Application.Queries;
using Order.Domain.Repositories;

namespace Order.Application.Handlers.Queries;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        return order == null ? null : OrderMapper.ToDto(order);
    }
}
