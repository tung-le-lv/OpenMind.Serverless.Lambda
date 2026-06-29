# Lambda Test Tool — Request Events

## CreateOrder

```json
{
  "httpMethod": "POST",
  "path": "/orders",
  "pathParameters": null,
  "queryStringParameters": null,
  "headers": { "Content-Type": "application/json" },
  "body": "{\"customerId\":\"cust-1\",\"items\":[{\"productId\":\"prod-1\",\"productName\":\"Wireless Mouse\",\"quantity\":2,\"unitPrice\":29.99},{\"productId\":\"prod-2\",\"productName\":\"Mechanical Keyboard\",\"quantity\":1,\"unitPrice\":89.99}],\"shippingAddress\":{\"street\":\"123 Main St\",\"city\":\"Sydney\",\"state\":\"NSW\",\"zipCode\":\"2000\",\"country\":\"Australia\"}}",
  "requestContext": { "requestId": "test-create-order" }
}
```

## GetAllOrders

```json
{
  "httpMethod": "GET",
  "path": "/orders",
  "pathParameters": null,
  "queryStringParameters": null,
  "headers": {},
  "body": null,
  "requestContext": { "requestId": "test-get-all-orders" }
}
```

## GetOrder

```json
{
  "httpMethod": "GET",
  "path": "/orders/order-sample-1",
  "pathParameters": { "id": "order-sample-1" },
  "queryStringParameters": null,
  "headers": {},
  "body": null,
  "requestContext": { "requestId": "test-get-order" }
}
```

## GetOrdersByCustomer

```json
{
  "httpMethod": "GET",
  "path": "/orders/customer/cust-1",
  "pathParameters": { "customerId": "cust-1" },
  "queryStringParameters": null,
  "headers": {},
  "body": null,
  "requestContext": { "requestId": "test-get-orders-by-customer" }
}
```

## GetOrdersByCustomerAndStatus

```json
{
  "httpMethod": "GET",
  "path": "/orders/customer/cust-1/status/Pending",
  "pathParameters": { "customerId": "cust-1", "status": "Pending" },
  "queryStringParameters": null,
  "headers": {},
  "body": null,
  "requestContext": { "requestId": "test-get-orders-by-customer-and-status" }
}
```

## GetOrdersByDateRange

```json
{
  "httpMethod": "GET",
  "path": "/orders/filter",
  "pathParameters": null,
  "queryStringParameters": { "date": "2026-06-15" },
  "headers": {},
  "body": null,
  "requestContext": { "requestId": "test-get-orders-by-date-range" }
}
```

## AddOrderItem

```json
{
  "httpMethod": "POST",
  "path": "/orders/order-sample-1/items",
  "pathParameters": { "id": "order-sample-1" },
  "queryStringParameters": null,
  "headers": { "Content-Type": "application/json" },
  "body": "{\"productId\":\"prod-9\",\"productName\":\"USB Hub\",\"quantity\":1,\"unitPrice\":19.99}",
  "requestContext": { "requestId": "test-add-order-item" }
}
```

## UpdateOrderStatus

```json
{
  "httpMethod": "PUT",
  "path": "/orders/order-sample-2/status",
  "pathParameters": { "id": "order-sample-2" },
  "queryStringParameters": null,
  "headers": { "Content-Type": "application/json" },
  "body": "{\"newStatus\": \"Confirmed\"}",
  "requestContext": { "requestId": "test-update-order-status" }
}
```

## CancelOrder

```json
{
  "httpMethod": "POST",
  "path": "/orders/order-sample-3/cancel",
  "pathParameters": { "id": "order-sample-3" },
  "queryStringParameters": null,
  "headers": {},
  "body": null,
  "requestContext": { "requestId": "test-cancel-order" }
}
```

## DeleteOrder

```json
{
  "httpMethod": "DELETE",
  "path": "/orders/order-sample-5",
  "pathParameters": { "id": "order-sample-5" },
  "queryStringParameters": null,
  "headers": {},
  "body": null,
  "requestContext": { "requestId": "test-delete-order" }
}
```
