using MediatR;
using Order.Api.Domain.Repositories;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrdersByCustomerAndStatus;

public class GetOrdersByCustomerAndStatusQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersByCustomerAndStatusQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByCustomerAndStatusQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByCustomerIdAndStatusAsync(request.CustomerId, request.Status, cancellationToken);
        return orders.Select(OrderMapper.ToDto);
    }
}
