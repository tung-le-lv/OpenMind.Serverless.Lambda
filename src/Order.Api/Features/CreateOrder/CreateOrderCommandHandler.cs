using MediatR;
using Order.Api.Domain;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Shared.Application.Interfaces;

namespace Order.Api.Features.CreateOrder;

public class CreateOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = OrderAggregate.Create(request.CustomerId);

            foreach (var item in request.Items)
            {
                order.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
            }

            await orderRepository.AddAsync(order, cancellationToken);

            foreach (var domainEvent in order.DomainEvents)
            {
                await eventBus.PublishAsync(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            return new CreateOrderResult(true, order.Id, "Order created successfully.", null);
        }
        catch (DomainException ex)
        {
            return new CreateOrderResult(false, null, "Domain validation failed.", [ex.Message]);
        }
        catch (Exception ex)
        {
            return new CreateOrderResult(false, null, "An error occurred while creating the order.", [ex.Message]);
        }
    }
}
