using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;

namespace Order.Api.Features.CreateOrder;

public partial class CreateOrderFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddEventBus();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<IRequestHandler<CreateOrderCommand, CreateOrderResult>, CreateOrderHandler>();
        services.AddTransient<IValidator<CreateOrderCommand>, CreateOrderValidator>();
        return services.BuildServiceProvider();
    }
}
