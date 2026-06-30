using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Application.Interfaces;
using Order.Api.Domain.Repositories;
using Order.Api.Infrastructure.EventBus;
using Order.Api.Infrastructure.Repositories;

namespace Order.Api.Shared;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DynamoDbOrderRepository).Assembly));
        
        return services;
    }

    internal static IServiceCollection AddEventBus(this IServiceCollection services)
    {
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
        
        return services;
    }
}
