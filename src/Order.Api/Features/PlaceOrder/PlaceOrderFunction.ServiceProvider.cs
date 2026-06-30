using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;

namespace Order.Api.Features.PlaceOrder;

public partial class PlaceOrderFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddEventBus();
        services.AddTransient<IRequestHandler<PlaceOrderCommand, PlaceOrderResult>, PlaceOrderHandler>();
        return services.BuildServiceProvider();
    }
}
