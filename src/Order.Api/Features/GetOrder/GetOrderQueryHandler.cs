using MediatR;
using Order.Api.Domain.Repositories;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrder;

public class GetOrderQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        return order == null ? null : OrderMapper.ToDto(order);
    }
}
