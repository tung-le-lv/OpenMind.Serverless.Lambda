using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Events;

namespace Payment.Api.Domain.Entities;

public class PaymentAggregate
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public string Id { get; private init; } = string.Empty;
    public string OrderId { get; private init; } = string.Empty;
    public string CustomerId { get; private init; } = string.Empty;
    public decimal Amount { get; private init; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime ProcessedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static PaymentAggregate Create(string orderId, string customerId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new PaymentDomainException("Order ID is required.");
        }

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new PaymentDomainException("Customer ID is required.");
        }

        if (amount <= 0)
        {
            throw new PaymentDomainException("Payment amount must be greater than zero.");
        }

        return new PaymentAggregate
        {
            Id = Guid.NewGuid().ToString(),
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };
    }

    public static PaymentAggregate Reconstitute(
        string id,
        string orderId,
        string customerId,
        decimal amount,
        PaymentStatus status,
        string? failureReason,
        DateTime createdAt,
        DateTime processedAt)
    {
        return new PaymentAggregate
        {
            Id = id,
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            Status = status,
            FailureReason = failureReason,
            CreatedAt = createdAt,
            ProcessedAt = processedAt
        };
    }

    public void MarkAsProcessed()
    {
        Status = PaymentStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        _domainEvents.Add(new PaymentProcessedEvent(Id, OrderId, CustomerId, Amount));
    }

    public void MarkAsFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;
        _domainEvents.Add(new PaymentFailedEvent(Id, OrderId, reason));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public class PaymentDomainException(string message) : Exception(message);
