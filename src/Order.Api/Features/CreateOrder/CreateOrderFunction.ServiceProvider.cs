using Amazon.DynamoDBv2;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Domain.Repositories;
using Order.Api.Infrastructure.Repositories;
using Order.Api.Shared;

namespace Order.Api.Features.CreateOrder;

public partial class CreateOrderFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DynamoDbOrderRepository).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<IRequestHandler<CreateOrderCommand, CreateOrderResult>, CreateOrderHandler>();
        services.AddTransient<IValidator<CreateOrderCommand>, CreateOrderValidator>();
        return services.BuildServiceProvider();
    }
}
