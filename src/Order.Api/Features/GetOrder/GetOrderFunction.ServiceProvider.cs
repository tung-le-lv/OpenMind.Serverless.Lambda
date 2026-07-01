using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Domain.Repositories;
using Order.Api.Shared;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrder;

public partial class GetOrderFunction
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddTransient<IRequestHandler<GetOrderQuery, OrderDto?>, GetOrderQueryHandler>();
        return services.BuildServiceProvider();
    }
}
