using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Application.Dtos;
using Order.Api.Domain.Repositories;
using Order.Api.Shared;

namespace Order.Api.Features.GetOrdersByCustomerAndStatus;

public partial class GetOrdersByCustomerAndStatusFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddTransient<IRequestHandler<GetOrdersByCustomerAndStatusQuery, IEnumerable<OrderDto>>, GetOrdersByCustomerAndStatusHandler>();
        return services.BuildServiceProvider();
    }
}
