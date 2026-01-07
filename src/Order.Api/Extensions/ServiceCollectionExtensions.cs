using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Handlers.Commands;
using Order.Application.Interfaces;
using Order.Application.Validators;
using Order.Domain.Repositories;
using Order.Infrastructure.EventBus;
using Order.Infrastructure.Repositories;

namespace Order.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services)
    {
        // AWS Services
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();

        // Repositories
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();

        // Event Bus
        var useLocalEventBus = Environment.GetEnvironmentVariable("USE_LOCAL_EVENT_BUS") == "true";
        if (useLocalEventBus)
        {
            services.AddSingleton<IEventBus, InMemoryEventBus>();
        }
        else
        {
            services.AddSingleton<IEventBus, SnsEventBus>();
        }

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly));

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();

        return services;
    }
}
