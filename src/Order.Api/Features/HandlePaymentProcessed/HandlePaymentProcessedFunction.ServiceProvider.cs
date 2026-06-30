using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Features.UpdateOrderStatus;
using Order.Api.Shared;

namespace Order.Api.Features.HandlePaymentProcessed;

public partial class HandlePaymentProcessedFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddEventBus();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>, UpdateOrderStatusCommandHandler>();
        return services.BuildServiceProvider();
    }
}
