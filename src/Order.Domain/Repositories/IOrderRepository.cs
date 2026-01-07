using Order.Domain.Entities;

namespace Order.Domain.Repositories;

public interface IOrderRepository
{
    Task<OrderEntity?> GetByIdAsync(string orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderEntity> AddAsync(OrderEntity order, CancellationToken cancellationToken = default);
    Task<OrderEntity> UpdateAsync(OrderEntity order, CancellationToken cancellationToken = default);
    Task DeleteAsync(string orderId, CancellationToken cancellationToken = default);
}
