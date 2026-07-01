# Running Integration Local

This local run heavily depends on localstack. Used for testing integration between lambda functions.  
```
POST /orders/{id}/place
  └─ PlaceOrderFunction (Lambda)
       └─ publishes OrderPlaced → SNS: order-events-topic-local
            └─ fans out → SQS: payment-order-events-sqs-local
                 └─ ProcessPaymentFunction (Lambda)
                      └─ publishes PaymentProcessed → SNS: payment-events-topic-local
                           └─ fans out → SQS: order-payment-events-sqs-local
                                └─ HandlePaymentProcessedFunction (Lambda)
```

All setup relating to SNS, SQS, Lamda, etc are defined in localstack-http-setup and localstack-setup in docker-composed.


## After changing code

```powershell
.\deploy\local\build.ps1
podman compose down
podman compose up -d
```

## Start infrastructure

```powershell
winget install Docker.DockerCompose   # required by podman compose
```

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
| `ProcessPaymentFunction` | registered in LocalStack, triggered by `payment-order-events-sqs-local` |
| `HandlePaymentProcessedFunction` | registered in LocalStack, triggered by `order-payment-events-sqs-local` |

---

## Build Lambda ZIPs

Run after any code change:

```powershell
.\deploy\local\build.ps1
```

This publishes two self-contained linux-x64 ZIPs — one per bounded context, not one per function. All Lambda functions within the same project share the same ZIP; the `LAMBDA_HANDLER` environment variable tells LocalStack which handler to invoke at runtime.

| ZIP | Lambda functions inside |
|---|---|
| `publish/order-api.zip` | `CreateOrderFunction`, `PlaceOrderFunction`, `HandlePaymentProcessedFunction`, and all other Order API functions |
| `publish/payment-api.zip` | `ProcessPaymentFunction` |

The Lambda functions in LocalStack are registered by the setup containers (localstack-setup and localstack-http-setup) which only run once at startup — a full down && up re-runs them so LocalStack picks up the new ZIPs.

```powershell
podman compose down
podman compose up -d
```

## API Gateway base URL

```powershell
podman compose logs localstack-http-setup | Select-String "Base URL"
```

The URL looks like: `http://localhost:4566/restapis/abc1234567/local/_user_request_`

---

## Logs

```powershell
podman compose logs -f localstack-logs
```

Or this is more reliable. Each lambda function has its own logs.  
Just paste the whole script to powershell.

```powershell
$env:AWS_ACCESS_KEY_ID = "test"
$env:AWS_SECRET_ACCESS_KEY = "test"
$env:AWS_DEFAULT_REGION = "ap-southeast-2"

function Show-LambdaLogs($functionName) {
    $result = aws --endpoint-url=http://localhost:4566 `
        logs filter-log-events `
        --log-group-name "/aws/lambda/$functionName" `
        --start-time 0 | ConvertFrom-Json

    $result.events | ForEach-Object {
        $ts = [DateTimeOffset]::FromUnixTimeMilliseconds($_.timestamp).ToString("HH:mm:ss")
        $msg = $_.message.Trim()
        if ($msg -match '^\{') {
            try {
                $json = $msg | ConvertFrom-Json
                $level = $json.level
                $text  = $json.message
                if ($text) {
                    $color = if ($level -eq 'Error') { 'Red' } elseif ($level -eq 'Warning') { 'Yellow' } else { 'Cyan' }
                    Write-Host "$ts [$level] $text" -ForegroundColor $color
                }
            } catch {}
        } elseif ($msg -match '^(START|END|REPORT)') {
            Write-Host "$ts $msg" -ForegroundColor DarkGray
        }
    }
}

Show-LambdaLogs ProcessPaymentFunction
```

Sample logs:
```powershell
PS C:\Data\OpenMind-Public\OpenMind.Serverless> Show-LambdaLogs PlaceOrderFunction
07:55:26 START RequestId: 44fc32ec-7d38-42b1-8e82-fd207b48734f Version: $LATEST
07:55:26 [Information] Placing order 3b736b1e-1bdf-41f9-9d95-c604f82d5efd
07:55:26 END RequestId: 44fc32ec-7d38-42b1-8e82-fd207b48734f
07:55:26 REPORT RequestId: 44fc32ec-7d38-42b1-8e82-fd207b48734f Duration: 358.60 ms     Billed Duration: 359 ms Memory Size: 128 MB     Max Memory Used: 128 MB

PS C:\Users\tung.le> Show-LambdaLogs ProcessPaymentFunction
07:55:28 START RequestId: be58374a-7827-4575-ba7c-28479efb0660 Version: $LATEST
07:55:28 [Information] Processing payment for order 3b736b1e-1bdf-41f9-9d95-c604f82d5efd, customer cust-11, amount 149.97
07:55:28 [Information] Payment processed for order 3b736b1e-1bdf-41f9-9d95-c604f82d5efd, success: True
07:55:28 END RequestId: be58374a-7827-4575-ba7c-28479efb0660
07:55:28 REPORT RequestId: be58374a-7827-4575-ba7c-28479efb0660 Duration: 194.41 ms     Billed Duration: 195 ms Memory Size: 128 MB     Max Memory Used: 128 MB
```

## Send requests

Can use postman.

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

### Place the order

```http
POST {BASE_URL}/orders/{orderId}/place
```
