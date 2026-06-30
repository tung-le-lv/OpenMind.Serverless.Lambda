using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Application.Dtos;
using Order.Api.Domain.Repositories;
using Order.Api.Shared;

namespace Order.Api.Features.GetOrdersByCustomer;

public partial class GetOrdersByCustomerFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddTransient<IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<OrderDto>>, GetOrdersByCustomerHandler>();
        return services.BuildServiceProvider();
    }
}
