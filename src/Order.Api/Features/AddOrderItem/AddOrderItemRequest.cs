namespace Order.Api.Features.AddOrderItem;

public record AddOrderItemRequest(string ProductId, string ProductName, int Quantity, decimal UnitPrice);
