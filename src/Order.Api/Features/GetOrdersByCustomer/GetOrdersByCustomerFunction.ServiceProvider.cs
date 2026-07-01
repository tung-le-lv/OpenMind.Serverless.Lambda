using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrdersByCustomer;

public partial class GetOrdersByCustomerFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddTransient<IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<OrderDto>>, GetOrdersByCustomerQueryHandler>();
        return services.BuildServiceProvider();
    }
}
