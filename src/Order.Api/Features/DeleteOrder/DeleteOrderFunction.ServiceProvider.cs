using Amazon.DynamoDBv2;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Domain.Repositories;
using Order.Api.Infrastructure.Repositories;

namespace Order.Api.Features.DeleteOrder;

public partial class DeleteOrderFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DynamoDbOrderRepository).Assembly));
        services.AddTransient<IRequestHandler<DeleteOrderCommand, DeleteOrderResult>, DeleteOrderHandler>();
        return services.BuildServiceProvider();
    }
}
