using Payment.Api.Application.Interfaces;
using Payment.Api.Domain.Entities;

namespace Payment.Api.Infrastructure.PaymentGateway;

public class FakePaymentGateway : IPaymentGateway
{
    public Task<bool> ChargeAsync(PaymentAggregate payment, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
