using MediatR;
using Payment.Api.Domain;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Repositories;
using Payment.Api.Shared.Application.Interfaces;

namespace Payment.Api.Features.ProcessPayment;

public class ProcessPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IPaymentGateway paymentGateway,
    IEventBus eventBus
) : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    public async Task<ProcessPaymentResult> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = PaymentAggregate.Create(request.OrderId, request.CustomerId, request.Amount);

            var success = await paymentGateway.ChargeAsync(payment, cancellationToken);

            if (success)
            {
                payment.MarkAsProcessed();
            }
            else
            {
                payment.MarkAsFailed("Payment gateway declined the transaction.");
            }

            await paymentRepository.AddAsync(payment, cancellationToken);

            foreach (var domainEvent in payment.DomainEvents)
            {
                await eventBus.PublishAsync(domainEvent, cancellationToken);
            }
            payment.ClearDomainEvents();

            return new ProcessPaymentResult(success, payment.Id, success ? "Payment processed successfully." : "Payment declined.");
        }
        catch (DomainException ex)
        {
            return new ProcessPaymentResult(false, null, ex.Message);
        }
    }
}
