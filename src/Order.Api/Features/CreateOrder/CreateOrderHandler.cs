using MediatR;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Interfaces;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Shared;

namespace Order.Api.Features.CreateOrder;

public class CreateOrderHandler(IOrderRepository orderRepository, IEventBus eventBus)
    : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Address? shippingAddress = null;
            if (request.ShippingAddress != null)
            {
                shippingAddress = Address.Create(
                    request.ShippingAddress.Street,
                    request.ShippingAddress.City,
                    request.ShippingAddress.State,
                    request.ShippingAddress.ZipCode,
                    request.ShippingAddress.Country
                );
            }

            var order = OrderAggregate.Create(request.CustomerId, shippingAddress);

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
