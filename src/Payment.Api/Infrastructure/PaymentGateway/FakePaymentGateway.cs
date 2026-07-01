using Payment.Api.Domain.Entities;
using Payment.Api.Shared.Application.Interfaces;

namespace Payment.Api.Infrastructure.PaymentGateway;

public class FakePaymentGateway : IPaymentGateway
{
    public Task<bool> ChargeAsync(PaymentAggregate payment, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
