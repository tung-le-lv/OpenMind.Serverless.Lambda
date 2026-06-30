using Payment.Api.Domain.Entities;

namespace Payment.Api.Domain.Repositories;

public interface IPaymentRepository
{
    Task<PaymentAggregate?> GetByIdAsync(string paymentId, CancellationToken cancellationToken = default);
    Task<PaymentAggregate?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
    Task<PaymentAggregate> AddAsync(PaymentAggregate payment, CancellationToken cancellationToken = default);
}
