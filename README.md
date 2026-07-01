# Serverless Architecture and Function as a Service

## Table of Contents

- [Overview](#overview)
- [Design Strategy](#design-strategy)
- [AWS Services for Serverless Architecture](#aws-services-for-serverless-architecture)
- [SNS-SQS](#sns-sqs)
  - [Pattern: Fan-out via SNS → SQS](#pattern-fan-out-via-sns--sqs)
  - [Standard vs FIFO](#standard-vs-fifo)
  - [Message Ordering with FIFO](#message-ordering-with-fifo)
  - [Lambda Auto-Scaling from SQS](#lambda-auto-scaling-from-sqs)
- [DynamoDB](#dynamodb)
  - [Partition key](#partition-key)
  - [Partition key + Sort key](#partition-key--sort-key)
  - [Global Secondary Index](#global-secondary-index)
  - [Local Secondary Index](#local-secondary-index)
  - [Scan Request](#scan-request)
  - [UI Tool](#ui-tool)
- [Run](#run)
- [References](#references)

## Overview

**Serverless Architecture** is a design philosophy where you build systems entirely from managed cloud services, where you never provision or operate infrastructure yourself. The cloud provider handles servers, OS patching, scaling, and availability. You pay only for what you consume and idle resources cost nothing. Scaling is special, where resources can be scaled to down Zero.

**Function as a Service (FaaS)** is the *compute* layer of a serverless architecture. It is one specific piece — the part that runs your code. AWS Lambda is the FaaS offering on AWS. You deploy a function, define what triggers it, and the platform runs it on demand in a short-lived container.

> Serverless Architecture contains FaaS. FaaS is not the whole of serverless.

**Key constraints of FaaS to design around:**
- **Stateless** — no in-memory state survives between invocations. Persist everything to DynamoDB, S3, or ElastiCache.
- **Cold starts** — the first invocation after idle spins up a new container (~200–500 ms for .NET). Subsequent calls reuse the warm container.
- **Short-lived** — AWS Lambda enforces a 15-minute maximum timeout. Long-running work (report generation, bulk imports) belongs in ECS Tasks or Step Functions.
- **Event-driven** — functions are triggered by something: an HTTP request, a queue message, a schedule, a file upload. There is no "always-on" process.

## Design Strategy

Functions should be grouped by Bounded Context (BC). In general, each BC should be owned by a single team and have its own Git repository.  

BC is a self-contained business domain with its own ubiquitous language, domain model, and team ownership. Visit https://github.com/tung-le-lv/OpenMind.DDD.Patterns for more.  

In an e-commerce platform, we typically have the following BCs:

```
order/          ← order lifecycle, line items, status
catalog/        ← product listings, inventory levels, pricing
customer/       ← accounts, addresses, loyalty points
payment/        ← charge, refund, payment method management
notification/   ← email, SMS, push notifications
```

**Within a repo, one function per use case.** Each AWS Lambda function corresponds to one business operation. In this repo each feature folder under `Features/` maps to a separately deployable Lambda:

```
Features/
  CreateOrder/       → CreateOrderFunction  (POST /orders)
  AddOrderItem/      → AddOrderItemFunction (POST /orders/{id}/items)
  CancelOrder/       → CancelOrderFunction  (DELETE /orders/{id})
  GetOrder/          → GetOrderFunction     (GET /orders/{id})
  UpdateOrderStatus/ → UpdateOrderStatusFunction
```

Note that I made Payment BC part of the same git repo as Order BC for demonstration purpose.

## AWS Services for Serverless Architecture

| Layer | AWS Service | Why it fits |
|---|---|---|
| **Compute** | Lambda | Runs functions on demand; scales from zero to thousands of concurrent executions automatically |
| **Database** | DynamoDB | Scales at the table level; fully managed NoSQL; scales with Lambda automatically; no connection pool to manage (critical for FaaS) |
| **Async messaging** | SNS/SQS | Fan-out pub/sub |
| **File storage** | S3 | Object storage |
| **Orchestration** | Step Functions | Coordinates multi-step workflows (e.g. order → payment → fulfillment → notification) with retries, timeouts, and branching. This is actually a Process Manager or Saga Orchestator |
| **REST API** | API Gateway | Managed HTTP/REST/WebSocket endpoint; routes requests to Lambda without running a web server |

## SNS-SQS

### Pattern: Fan-out via SNS → SQS

```
[Service A] ──publish──▶ [SNS Topic] ──subscribe──▶ [SQS Queue A] ──trigger──▶ [Lambda A]
                                     └──subscribe──▶ [SQS Queue B] ──trigger──▶ [Lambda B]
```

### Standard vs FIFO

| | Standard | FIFO |
|---|---|---|
| Message ordering | Not guaranteed | Guaranteed per message group |
| Duplicate delivery | Possible | Deduplicated (5-min window) |

### Message Ordering with FIFO

With a **FIFO** queue:

1. **Publisher** sets `MessageGroupId = AggregateId` on each SNS publish call
2. **SNS FIFO** preserves and propagates `MessageGroupId` to all subscribed SQS FIFO queues
3. **SQS FIFO** enforces strict ordering within each group — no two messages with the same `MessageGroupId` are in-flight simultaneously
4. **Lambda** processes at most **one message per group at a time**, regardless of how many Lambda instances are running

Events for aggregate with different IDs (different `MessageGroupId`) are fully parallel.

If a message in group `order-A` fails and is retried, subsequent messages for `order-A` are blocked until the retry resolves. Messages for `order-B`, `order-C`, etc. are unaffected.

![SNS-SQS](docs/sns-sqs.jpg)

See [docs/sns-sqs-ordering.excalidraw](docs/sns-sqs-ordering.excalidraw).

### Lambda Auto-Scaling from SQS

SQS Standard: Lambda instances are automatically scaled based on queue depth.  
SQS FIFO: Lambda instances are automatically scaled based on number of active message groups.  

## DynamoDB

**Table Design**

| `OrderId` | `CustomerId` | `OrderDate` | `LineItems` |
| --- | --- | --- | --- |
| order-001 | cus-001 | 2025-1-1 | [ { "ProductId": "prod-1", "ProductName": "Widget", "Quantity": 2, "UnitPrice": 9.99 }] |
| order-002 | cus-001 | 2025-1-2 | [ { "ProductId": "prod-2", "ProductName": "Widget 2", "Quantity": 1, "UnitPrice": 8 }] |
| order-003 | cus-002 | 2025-1-2 | [ { "ProductId": "prod-1", "ProductName": "Widget", "Quantity": 2, "UnitPrice": 9.99 }] |

### Partition key

`OrderId = PartitionKey`

`OrderId`  is unique across the table → Primary key

Scenario: Query pattern is for retrieving details for single order.

```csharp
return await _dynamoDbClient.GetItemAsync(new GetItemRequest
{
    TableName = "Orders",
    Key = new Dictionary<string, AttributeValue>
    {
        { "OrderId", new AttributeValue { S = orderId } }
    }
});
```

### Partition key + Sort key

On a base table, keys must be unique:

- Partition key only → one item per partition-key value.
- Partition key + sort key → the combination must be unique → one partition-key value can hold multiple items.

`CustomerId` = Partition Key
`OrderDate` = Sort Key
Primary key = `CustomerId + OrderDate`

Scenario:

- Frequently need to get all orders for a single customer.
- Query for a specific order by `CustomerId + OrderDate`.
- This setup means the `CustomerId` partition contains all of that customer's orders in sorted order by `OrderDate`.

Query all orders for customer:

```csharp
var request = new QueryRequest
{
    TableName = "Orders",
    KeyConditionExpression = "CustomerId = :customerId",
    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    {
        { ":customerId", new AttributeValue { S = "CUST123" } }
    }
};

var response = await _dynamoDbClient.QueryAsync(request);
```

Query exact customer + order:

```csharp
var request = new QueryRequest
{
    TableName = "Orders",
    KeyConditionExpression = "CustomerId = :customerId AND OrderDate = :orderDate",
    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    {
        { ":customerId", new AttributeValue { S = "CUST001" } },
        { ":orderDate",  new AttributeValue { S = "2024-01-15" } }
    },
    ConsistentRead = true
};

var response = await _dynamoDbClient.QueryAsync(request);
```

### Global Secondary Index

Global Secondary Index (GSI) lets you query by attributes that are not the base table's primary key.

- A GSI is a read-only projection cloned from the base table, kept in sync asynchronously. You write to the base table, never to the GSI directly.
- GSI reads are eventually consistent only.
- GIS read model does not clone all properties from the base table unless you specify them (use **`INCLUDE`).**

```powershell
aws --endpoint-url=http://dynamodb-local:8000 dynamodb create-table -global-secondary-indexes '[{"IndexName":"CustomerIdIndex","KeySchema":[{"AttributeName":"customerId","KeyType":"HASH"}],"Projection":{"ProjectionType":"ALL"}}]'
```

- GSI keys are not required to be unique (neither partition key, sort key, nor the combination).
- GSI can be added or removed at any time after table creation.
- Can create up to 20 GSI per table.
- It requires IndexName (GSI name) in the query request.

Example. Create GSI:

- Partition Key: `OrderDate`
- Sort Key: `OrderId`

The base table uses `OrderId` as its partition key, so "find all orders on a given date" would require a full table `Scan`. Adding a GSI with `OrderDate` as the partition key turns that into an indexed `Query`.

Items are placed into partitions by their partition key (`OrderDate`), so all orders on the same date are co-located automatically. Note that GSI keys are not required to be unique: many orders can share the same `OrderDate`, and the `Query` returns all of them.

The `OrderId` sort key lets you:

- narrow a query to a specific order or a range of `OrderId`s within a date,
- return results in a predictable, sorted order, paginate stably.

```csharp
public async Task<List<Dictionary<string, AttributeValue>>>
    QueryOrdersByDateAsync(DateTime orderDate)
{
    var request = new QueryRequest
    {
        TableName = "Orders",
        IndexName = "OrderDate-OrderId-index",
        KeyConditionExpression = "OrderDate = :pk",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":pk", new AttributeValue { S = orderDate.ToString("yyyy-MM-dd") } }
        }
    };

    return await _dynamoDbClient.QueryAsync(request);
}
```

### Local Secondary Index

Allow query on an alternate sort key, but same partition key as the base table. LSI must be defined when table is created.

![lsi](docs/lsi.png)

Create Local Secondary Index:

- Partition Key: `CustomerId` (same as base table)
- Sort Key: `OrderTotal`

```csharp
// Search cus-001's orders with total between 100 and 500, sorted by OrderTotal descending
var request = new QueryRequest
{
    TableName = "Orders",
    IndexName = "CustomerId-OrderTotal-index", // LSI name
    KeyConditionExpression = "CustomerId = :customerId AND OrderTotal BETWEEN :min AND :max",
    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    {
        { ":customerId", new AttributeValue { S = "CUST001" } },
        { ":min", new AttributeValue { N = "100" } },
        { ":max", new AttributeValue { N = "500" } }
    },
    ScanIndexForward = false  // largest totals first; omit or set true for ascending
};

var response = await _dynamoDbClient.QueryAsync(request);
```

### Scan Request

Can retrieve any data from table but need to scan the entire table.

```csharp
var request = new ScanRequest
{
    TableName = "Orders",
    FilterExpression = "OrderStatus = :status",
    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    {
        { ":status", new AttributeValue { S = "Completed" } }
    }
};

return await _dynamoDbClient.ScanAsync(request);
```

—> Better solution is to add a **GSI with `OrderStatus` as the partition key:**

```jsx
var request = new QueryRequest
{
    TableName = "Orders",
    IndexName = "OrderStatus-index",
    KeyConditionExpression = "OrderStatus = :status",
    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    {
        { ":status", new AttributeValue { S = "Completed" } }
    }
};

return await _dynamoDbClient.QueryAsync(request);
```

### UI Tool
Use [NoSQL Workbench](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/workbench.html) to browse data locally.

Add a connection: **Operation Builder → Add Connection → DynamoDB Local → hostname `localhost`, port `8000`**.

![NoSQL Workbench](docs/aws-workbench.jpg)

## Run

Refer [RunIntegrationLocal.md](RunIntegrationLocal.md) and [RunLocalAndDebug.md](RunLocalAndDebug.md).

## References
https://github.com/serverless/examples/tree/master/aws-dotnet-rest-api-with-dynamodb
