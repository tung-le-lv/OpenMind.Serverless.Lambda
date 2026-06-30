namespace Payment.Api.Features.ProcessPayment;

public record OrderPlacedData(string OrderId, string CustomerId, decimal TotalAmount);