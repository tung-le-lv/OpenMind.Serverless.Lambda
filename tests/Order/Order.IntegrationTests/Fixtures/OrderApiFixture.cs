using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Application.Interfaces;
using Order.Api.Domain.Repositories;
using Order.Api.Features.AddOrderItem;
using Order.Api.Features.CreateOrder;
using Order.Api.Infrastructure.EventBus;
using Order.Api.Infrastructure.Repositories;
using Order.Api.Shared;
using Testcontainers.DynamoDb;
using Xunit;

namespace Order.IntegrationTests.Fixtures;

public class OrderApiFixture : IAsyncLifetime
{
    private readonly DynamoDbContainer _container = new DynamoDbBuilder()
        .WithImage("amazon/dynamodb-local:latest")
        .Build();

    private ServiceProvider? _serviceProvider;

    public IMediator Mediator => _serviceProvider!.GetRequiredService<IMediator>();

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("USE_LOCAL_EVENT_BUS", "true");
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "test");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "test");
        Environment.SetEnvironmentVariable("AWS_DEFAULT_REGION", "us-east-1");
        Environment.SetEnvironmentVariable("POWERTOOLS_SERVICE_NAME", "test");
        Environment.SetEnvironmentVariable("POWERTOOLS_METRICS_NAMESPACE", "test");
        Environment.SetEnvironmentVariable("ORDERS_TABLE", "Orders");

        await _container.StartAsync();

        var config = new AmazonDynamoDBConfig { ServiceURL = _container.GetConnectionString() };
        var dynamoDbClient = new AmazonDynamoDBClient("test", "test", config);

        await CreateOrdersTable(dynamoDbClient);

        var services = new ServiceCollection();
        services.AddSingleton<IAmazonDynamoDB>(dynamoDbClient);
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DynamoDbOrderRepository).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<IValidator<CreateOrderCommand>, CreateOrderValidator>();
        services.AddTransient<IValidator<AddOrderItemCommand>, AddOrderItemValidator>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }
        await _container.DisposeAsync();
    }

    private static async Task CreateOrdersTable(IAmazonDynamoDB client)
    {
        await client.CreateTableAsync(new CreateTableRequest
        {
            TableName = "Orders",
            KeySchema = [new KeySchemaElement { AttributeName = "id", KeyType = KeyType.HASH }],
            AttributeDefinitions =
            [
                new AttributeDefinition { AttributeName = "id", AttributeType = ScalarAttributeType.S },
                new AttributeDefinition { AttributeName = "customerId", AttributeType = ScalarAttributeType.S },
                new AttributeDefinition { AttributeName = "orderDate", AttributeType = ScalarAttributeType.S }
            ],
            GlobalSecondaryIndexes =
            [
                new GlobalSecondaryIndex
                {
                    IndexName = "CustomerIdIndex",
                    KeySchema = [new KeySchemaElement { AttributeName = "customerId", KeyType = KeyType.HASH }],
                    Projection = new Projection { ProjectionType = ProjectionType.ALL },
                    ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 5, WriteCapacityUnits = 5 }
                },
                new GlobalSecondaryIndex
                {
                    IndexName = "OrderDateIndex",
                    KeySchema = [new KeySchemaElement { AttributeName = "orderDate", KeyType = KeyType.HASH }],
                    Projection = new Projection { ProjectionType = ProjectionType.ALL },
                    ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 5, WriteCapacityUnits = 5 }
                }
            ],
            ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 5, WriteCapacityUnits = 5 }
        });
    }
}

[CollectionDefinition("OrderApi")]
public class OrderApiCollection : ICollectionFixture<OrderApiFixture>;
