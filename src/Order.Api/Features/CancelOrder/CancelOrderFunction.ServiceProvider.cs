using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;

namespace Order.Api.Features.CancelOrder;

public partial class CancelOrderFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddEventBus();
        services.AddTransient<IRequestHandler<CancelOrderCommand, CancelOrderResult>, CancelOrderCommandHandler>();
        return services.BuildServiceProvider();
    }
}
