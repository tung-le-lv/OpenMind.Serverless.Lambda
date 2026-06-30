using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Domain.Repositories;
using Order.Api.Shared;

namespace Order.Api.Features.DeleteOrder;

public partial class DeleteOrderFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddTransient<IRequestHandler<DeleteOrderCommand, DeleteOrderResult>, DeleteOrderHandler>();
        return services.BuildServiceProvider();
    }
}
