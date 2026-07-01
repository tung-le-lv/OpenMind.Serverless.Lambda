using MediatR;
using Order.Api.Domain.Repositories;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrdersByCustomer;

public class GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        return orders.Select(OrderMapper.ToDto);
    }
}
