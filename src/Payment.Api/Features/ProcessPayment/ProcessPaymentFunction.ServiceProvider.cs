using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Payment.Api.Application.Interfaces;
using Payment.Api.Domain.Repositories;
using Payment.Api.Infrastructure.EventBus;
using Payment.Api.Infrastructure.PaymentGateway;
using Payment.Api.Infrastructure.Repositories;

namespace Payment.Api.Features.ProcessPayment;

public partial class ProcessPaymentFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        if (Environment.GetEnvironmentVariable("USE_LOCAL_EVENT_BUS") == "true")
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");
            var dynamoEndpoint = Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL") ?? "http://localhost:8000";
            var localstackEndpoint = Environment.GetEnvironmentVariable("LOCALSTACK_ENDPOINT") ?? "http://localhost:4566";
            var region = Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION") ?? "ap-southeast-2";
            services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(
                credentials, new AmazonDynamoDBConfig { ServiceURL = dynamoEndpoint, AuthenticationRegion = region }));
            services.AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient(
                credentials, new AmazonSimpleNotificationServiceConfig { ServiceURL = localstackEndpoint, AuthenticationRegion = region }));
        }
        else
        {
            services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
        }
        services.AddSingleton<IPaymentRepository, DynamoDbPaymentRepository>();
        services.AddSingleton<IPaymentGateway, FakePaymentGateway>();
        services.AddSingleton<IEventBus, SnsEventBus>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessPaymentFunction).Assembly));
        services.AddTransient<IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>, ProcessPaymentCommandHandler>();
        return services.BuildServiceProvider();
    }
}
