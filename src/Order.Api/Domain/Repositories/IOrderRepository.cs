using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;

namespace Order.Api.Domain.Repositories;

public interface IOrderRepository
{
    Task<OrderAggregate?> GetByIdAsync(string orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderAggregate>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderAggregate>> GetByCustomerIdAndStatusAsync(string customerId, OrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderAggregate>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderAggregate> AddAsync(OrderAggregate order, CancellationToken cancellationToken = default);
    Task<OrderAggregate> UpdateAsync(OrderAggregate order, CancellationToken cancellationToken = default);
    Task DeleteAsync(string orderId, CancellationToken cancellationToken = default);
}
