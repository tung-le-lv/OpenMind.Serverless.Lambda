using MediatR;

namespace Payment.Api.Features.ProcessPayment;

public record ProcessPaymentCommand(
    string OrderId,
    string CustomerId,
    decimal Amount
) : IRequest<ProcessPaymentResult>;

public record ProcessPaymentResult(bool Success, string? PaymentId, string? Message);
