# Running Locally

End-to-end guide: start the stack, send requests, and observe logs across all Lambda functions connected via SNS → SQS.

All services run via LocalStack — no SAM CLI or SSH tunnel required.

---

## Prerequisites

Install once:

```powershell
winget install Docker.DockerCompose   # required by podman compose
```

---

## Step 1 — Start infrastructure

```powershell
podman compose up -d
```

This starts and auto-configures:

| What | Where |
|---|---|
| DynamoDB Local | `localhost:8000` — `Orders-local` and `Payments-local` tables with GSIs and seed data |
| LocalStack | `localhost:4566` — SNS topics, SQS queues, Lambda registrations, API Gateway |
| `CreateOrderFunction` | registered in LocalStack, HTTP via API Gateway |
| `PlaceOrderFunction` | registered in LocalStack, HTTP via API Gateway |
| `ProcessPaymentFunction` | registered in LocalStack, triggered by `payment-order-events-sqs` |
| `HandlePaymentProcessedFunction` | registered in LocalStack, triggered by `order-payment-events-sqs` |

Wait until both setup services complete:

```powershell
podman compose logs -f localstack-setup localstack-http-setup
```

`localstack-http-setup` prints the API Gateway base URL at the end — note it for Step 3.

---

## Step 2 — Build Lambda images

Run after any code change:

```powershell
.\deploy\local\build.ps1
```

This builds two images — one per bounded context, not one per function. All Lambda functions within the same project share the same image; the `LAMBDA_HANDLER` environment variable tells LocalStack which handler to invoke at runtime.

| Image | Lambda functions inside |
|---|---|
| `order-api-local:latest` | `CreateOrderFunction`, `PlaceOrderFunction`, `HandlePaymentProcessedFunction`, and all other Order API functions |
| `payment-api-local:latest` | `ProcessPaymentFunction` |

---

## Step 3 — Get the API Gateway base URL

```powershell
podman compose logs localstack-http-setup | Select-String "Base URL"
```

The URL looks like: `http://localhost:4566/restapis/abc1234567/local/_user_request_`

---

## Step 4 — Watch logs (new terminal)

```powershell
podman compose logs -f localstack-logs
```

All four key functions stream here: `CreateOrderFunction`, `PlaceOrderFunction`, `ProcessPaymentFunction`, and `HandlePaymentProcessedFunction`.

> **First invocation per function is slow (~10–30 s)** while LocalStack starts the Lambda container. Subsequent calls are fast.

---

## Step 5 — Send requests

Replace `{BASE_URL}` with the URL from Step 3.

### Create an order

```http
POST {BASE_URL}/orders
Content-Type: application/json

{
  "customerId": "cust-1",
  "items": [
    { "productId": "prod-1", "productName": "Wireless Mouse", "quantity": 2, "unitPrice": 29.99 },
    { "productId": "prod-2", "productName": "Mechanical Keyboard", "quantity": 1, "unitPrice": 89.99 }
  ],
  "shippingAddress": {
    "street": "123 Main St", "city": "Sydney", "state": "NSW", "zipCode": "2000", "country": "Australia"
  }
}
```

Note the `orderId` from the response.

### Place the order (triggers the full event-driven flow)

```http
POST {BASE_URL}/orders/{orderId}/place
```

---

## What happens end-to-end

```
POST /orders/{id}/place
  └─ PlaceOrderFunction (LocalStack)
       └─ publishes OrderPlaced → SNS: order-events-topic-local
            └─ fans out → SQS: payment-order-events-sqs
                 └─ ProcessPaymentFunction (LocalStack)
                      └─ publishes PaymentProcessed → SNS: payment-events-topic-local
                           └─ fans out → SQS: order-payment-events-sqs
                                └─ HandlePaymentProcessedFunction (LocalStack)
```

---

## After changing code

```powershell
.\deploy\local\build.ps1
podman compose restart localstack
```

---

## Inspect DynamoDB

Use [NoSQL Workbench](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/workbench.html):

**Operation Builder → Add Connection → DynamoDB Local → hostname `localhost`, port `8000`**
