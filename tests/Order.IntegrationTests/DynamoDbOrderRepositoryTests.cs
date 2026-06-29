using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;
using Order.Api.Infrastructure.Repositories;
using Testcontainers.DynamoDb;
using Xunit;

namespace Order.IntegrationTests;

public class DynamoDbOrderRepositoryTests : IAsyncLifetime
{
    private readonly DynamoDbContainer _dynamoDbContainer = new DynamoDbBuilder()
        .WithImage("amazon/dynamodb-local:latest")
        .Build();
    private IAmazonDynamoDB? _dynamoDbClient;
    private DynamoDbOrderRepository? _repository;

    public async Task InitializeAsync()
    {
        await _dynamoDbContainer.StartAsync();

        var config = new AmazonDynamoDBConfig
        {
            ServiceURL = _dynamoDbContainer.GetConnectionString()
        };

        _dynamoDbClient = new AmazonDynamoDBClient("test", "test", config);

        // Create table
        await CreateOrdersTable();

        // Set environment variable for table name
        Environment.SetEnvironmentVariable("ORDERS_TABLE", "Orders");

        _repository = new DynamoDbOrderRepository(_dynamoDbClient);
    }

    public async Task DisposeAsync()
    {
        _dynamoDbClient?.Dispose();
        await _dynamoDbContainer.DisposeAsync();
    }

    private async Task CreateOrdersTable()
    {
        var request = new CreateTableRequest
        {
            TableName = "Orders",
            KeySchema =
            [
                new KeySchemaElement { AttributeName = "id", KeyType = KeyType.HASH }
            ],
            AttributeDefinitions =
            [
                new AttributeDefinition { AttributeName = "id", AttributeType = ScalarAttributeType.S },
                new AttributeDefinition { AttributeName = "customerId", AttributeType = ScalarAttributeType.S }
            ],
            GlobalSecondaryIndexes =
            [
                new GlobalSecondaryIndex
                {
                    IndexName = "CustomerIdIndex",
                    KeySchema = [new KeySchemaElement { AttributeName = "customerId", KeyType = KeyType.HASH }],
                    Projection = new Projection { ProjectionType = ProjectionType.ALL },
                    ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 5, WriteCapacityUnits = 5 }
                }
            ],
            ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 5, WriteCapacityUnits = 5 }
        };

        await _dynamoDbClient!.CreateTableAsync(request);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistOrder()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Seattle", "WA", "98101", "USA");
        var order = OrderAggregate.Create("customer-123", address);
        order.AddItem("prod-1", "Product 1", 2, 10.00m);

        // Act
        var result = await _repository!.AddAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var order = OrderAggregate.Create("customer-123");
        order.AddItem("prod-1", "Product 1", 2, 10.00m);
        await _repository!.AddAsync(order);

        // Act
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be("customer-123");
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByCustomerIdAsync_ShouldReturnCustomerOrders()
    {
        // Arrange
        var order1 = OrderAggregate.Create("customer-456");
        var order2 = OrderAggregate.Create("customer-456");
        await _repository!.AddAsync(order1);
        await _repository.AddAsync(order2);

        // Act
        var results = await _repository.GetByCustomerIdAsync("customer-456");

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveOrder()
    {
        var order = OrderAggregate.Create("customer-789");
        await _repository!.AddAsync(order);

        await _repository.DeleteAsync(order.Id);

        var result = await _repository.GetByIdAsync(order.Id);
        result.Should().BeNull();
    }
}
