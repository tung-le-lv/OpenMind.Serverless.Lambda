# Order Microservice - Clean Architecture with AWS Lambda

A serverless order microservice built with **Clean Architecture**, **CQRS pattern**, **DDD principles**, using **AWS Lambda**, **C#**, and **.NET 9**.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              API Gateway                                 │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Order.Api (Lambda Functions)                     │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐       │
│  │CreateOrder  │ │ GetOrder    │ │UpdateStatus │ │ CancelOrder │  ...  │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘       │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    Order.Application (CQRS + MediatR)                    │
│  ┌──────────────────────────┐  ┌──────────────────────────┐            │
│  │        Commands          │  │         Queries          │            │
│  │  • CreateOrderCommand    │  │  • GetOrderQuery         │            │
│  │  • UpdateStatusCommand   │  │  • GetAllOrdersQuery     │            │
│  │  • CancelOrderCommand    │  │  • GetOrdersByCustomer   │            │
│  │  • AddOrderItemCommand   │  │                          │            │
│  └──────────────────────────┘  └──────────────────────────┘            │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                       Order.Domain (DDD Entities)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                  │
│  │   Entities   │  │ Value Objects│  │Domain Events │                  │
│  │  • Order     │  │  • Money     │  │• OrderCreated│                  │
│  │  • OrderItem │  │  • Address   │  │• StatusChanged│                 │
│  └──────────────┘  └──────────────┘  └──────────────┘                  │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                       Order.Infrastructure                               │
│  ┌──────────────────────┐      ┌──────────────────────┐                │
│  │    DynamoDB Repo     │      │    SNS Event Bus     │                │
│  └──────────────────────┘      └──────────────────────┘                │
└─────────────────────────────────────────────────────────────────────────┘
                │                            │
                ▼                            ▼
        ┌───────────────┐           ┌───────────────┐
        │   DynamoDB    │           │   SNS Topic   │
        └───────────────┘           └───────────────┘
```

## Project Structure

```
OrderService/
├── src/
│   ├── Order.Api/                    # AWS Lambda Functions
│   │   ├── Functions/
│   │   │   ├── CreateOrder.cs
│   │   │   ├── GetOrder.cs
│   │   │   ├── GetAllOrders.cs
│   │   │   ├── GetOrdersByCustomer.cs
│   │   │   ├── UpdateOrderStatus.cs
│   │   │   ├── CancelOrder.cs
│   │   │   ├── AddOrderItem.cs
│   │   │   └── DeleteOrder.cs
│   │   └── Extensions/
│   │
│   ├── Order.Application/            # Use cases (CQRS)
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Handlers/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   ├── Mappers/
│   │   └── Interfaces/
│   │
│   ├── Order.Domain/                 # Domain layer (DDD)
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   ├── Events/
│   │   └── Repositories/
│   │
│   └── Order.Infrastructure/         # External concerns
│       ├── Repositories/
│       └── EventBus/
│
├── tests/
│   ├── Order.UnitTests/
│   └── Order.IntegrationTests/
│
├── deploy/
│   └── aws/
│       ├── template.yaml
│       ├── parameters.dev.json
│       └── parameters.prod.json
│
└── OrderService.sln
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/orders` | Create a new order |
| `GET` | `/orders` | Get all orders |
| `GET` | `/orders/{id}` | Get order by ID |
| `GET` | `/orders/customer/{customerId}` | Get orders by customer |
| `PUT` | `/orders/{id}/status` | Update order status |
| `POST` | `/orders/{id}/cancel` | Cancel an order |
| `POST` | `/orders/{id}/items` | Add item to order |
| `DELETE` | `/orders/{id}` | Delete an order |

## Domain Events

The service publishes domain events to SNS:
- `OrderCreated` - When a new order is created
- `OrderItemAdded` - When an item is added to an order
- `OrderStatusChanged` - When order status changes
- `OrderCancelled` - When an order is cancelled

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [AWS CLI](https://aws.amazon.com/cli/)
- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/serverless-sam-cli-install.html)
- [Docker](https://www.docker.com/) (for local testing and integration tests)

## Getting Started

### Build

```bash
dotnet build
```

### Run Tests

```bash
# Unit tests
dotnet test tests/Order.UnitTests

# Integration tests (requires Docker)
dotnet test tests/Order.IntegrationTests
```

### Local Development

```bash
cd deploy/aws

# Build
sam build

# Start local API
sam local start-api --parameter-overrides Environment=dev

# Test endpoints
curl http://localhost:3000/orders
```

## Deployment

### Deploy to AWS

```bash
cd deploy/aws

# Build
sam build

# Deploy (first time - guided)
sam deploy --guided

# Deploy to specific environment
sam deploy --parameter-overrides Environment=prod --config-file parameters.prod.json
```

## Sample Requests

### Create Order

```bash
curl -X POST https://your-api.execute-api.region.amazonaws.com/dev/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "cust-123",
    "items": [
      {
        "productId": "prod-001",
        "productName": "Widget A",
        "quantity": 2,
        "unitPrice": 29.99
      }
    ],
    "shippingAddress": {
      "street": "123 Main St",
      "city": "Seattle",
      "state": "WA",
      "zipCode": "98101",
      "country": "USA"
    }
  }'
```

### Update Order Status

```bash
curl -X PUT https://your-api.execute-api.region.amazonaws.com/dev/orders/{id}/status \
  -H "Content-Type: application/json" \
  -d '{"status": 1}'
```

### Add Item to Order

```bash
curl -X POST https://your-api.execute-api.region.amazonaws.com/dev/orders/{id}/items \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "prod-002",
    "productName": "Widget B",
    "quantity": 1,
    "unitPrice": 49.99
  }'
```

## Order Status Flow

```
Pending → Confirmed → Processing → Shipped → Delivered
    ↓          ↓           ↓
    └──────────┴───────────┴──────→ Cancelled
```

## Key Design Patterns

- **Clean Architecture**: Separation of concerns with clear boundaries
- **CQRS**: Commands and Queries separated for better scalability
- **DDD**: Rich domain model with entities, value objects, and domain events
- **MediatR**: Decoupled request/response handling
- **Repository Pattern**: Abstract data access
- **Event-Driven**: Domain events published via SNS

## License

MIT
