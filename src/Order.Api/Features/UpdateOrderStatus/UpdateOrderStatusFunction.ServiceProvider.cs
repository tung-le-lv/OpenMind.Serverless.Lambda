using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;

namespace Order.Api.Features.UpdateOrderStatus;

public partial class UpdateOrderStatusFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddEventBus();
        services.AddTransient<IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>, UpdateOrderStatusCommandHandler>();
        return services.BuildServiceProvider();
    }
}
