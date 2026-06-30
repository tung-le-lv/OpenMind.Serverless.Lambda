using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Application.Interfaces;
using Order.Api.Domain.Repositories;
using Order.Api.Infrastructure.EventBus;
using Order.Api.Infrastructure.Repositories;

namespace Order.Api.Features.CancelOrder;

public partial class CancelOrderFunction
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
            var region = Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION") ?? "ap-southeast-2";
            services.AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient(
                credentials, new AmazonSimpleNotificationServiceConfig { ServiceURL = localstackEndpoint, AuthenticationRegion = region }));
        }
        else
        {
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
        }
        services.AddSingleton<IEventBus, SnsEventBus>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DynamoDbOrderRepository).Assembly));
        services.AddTransient<IRequestHandler<CancelOrderCommand, CancelOrderResult>, CancelOrderHandler>();
        return services.BuildServiceProvider();
    }
}
