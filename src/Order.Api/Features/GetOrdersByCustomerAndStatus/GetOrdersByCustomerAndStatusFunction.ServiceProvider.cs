using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Domain.Repositories;
using Order.Api.Shared;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrdersByCustomerAndStatus;

public partial class GetOrdersByCustomerAndStatusFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddTransient<IRequestHandler<GetOrdersByCustomerAndStatusQuery, IEnumerable<OrderDto>>, GetOrdersByCustomerAndStatusQueryHandler>();
        return services.BuildServiceProvider();
    }
}
