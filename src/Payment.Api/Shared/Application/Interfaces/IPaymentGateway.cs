using Payment.Api.Domain.Entities;

namespace Payment.Api.Shared.Application.Interfaces;

public interface IPaymentGateway
{
    Task<bool> ChargeAsync(PaymentAggregate payment, CancellationToken cancellationToken = default);
}
