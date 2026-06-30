using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Application.Dtos;
using Order.Api.Domain.Repositories;
using Order.Api.Shared;

namespace Order.Api.Features.GetAllOrders;

public partial class GetAllOrdersFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddTransient<IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDto>>, GetAllOrdersHandler>();
        return services.BuildServiceProvider();
    }
}
