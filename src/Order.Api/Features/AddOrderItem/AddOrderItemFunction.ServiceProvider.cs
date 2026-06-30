using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;

namespace Order.Api.Features.AddOrderItem;

public partial class AddOrderItemFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddEventBus();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<IRequestHandler<AddOrderItemCommand, AddOrderItemResult>, AddOrderItemHandler>();
        services.AddTransient<IValidator<AddOrderItemCommand>, AddOrderItemValidator>();
        return services.BuildServiceProvider();
    }
}
