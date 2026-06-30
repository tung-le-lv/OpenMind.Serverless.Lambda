using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Application.Interfaces;
using Order.Api.Domain.Repositories;
using Order.Api.Infrastructure.EventBus;
using Order.Api.Infrastructure.Repositories;
using Order.Api.Shared;

namespace Order.Api.Features.AddOrderItem;

public partial class AddOrderItemFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();
        if (Environment.GetEnvironmentVariable("USE_LOCAL_EVENT_BUS") == "true")
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");
            var localstackEndpoint = Environment.GetEnvironmentVariable("LOCALSTACK_ENDPOINT") ?? "http://localhost:4566";
            services.AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient(
                credentials, new AmazonSimpleNotificationServiceConfig { ServiceURL = localstackEndpoint }));
        }
        else
        {
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
        }
        services.AddSingleton<IEventBus, SnsEventBus>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DynamoDbOrderRepository).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<IRequestHandler<AddOrderItemCommand, AddOrderItemResult>, AddOrderItemHandler>();
        services.AddTransient<IValidator<AddOrderItemCommand>, AddOrderItemValidator>();
        return services.BuildServiceProvider();
    }
}
