using MediatR;
using Order.Application.Commands;
using Order.Domain.Entities;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;
using Order.Application.Interfaces;

namespace Order.Application.Handlers.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create shipping address if provided
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

            // Create order entity
            var order = OrderEntity.Create(request.CustomerId, shippingAddress);

            // Add items
            foreach (var item in request.Items)
            {
                order.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
            }

            // Persist order
            await _orderRepository.AddAsync(order, cancellationToken);

            // Publish domain events
            foreach (var domainEvent in order.DomainEvents)
            {
                await _eventBus.PublishAsync(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            return new CreateOrderResult(true, order.Id, "Order created successfully.", null);
        }
        catch (DomainException ex)
        {
            return new CreateOrderResult(false, null, "Validation failed.", [ex.Message]);
        }
        catch (Exception ex)
        {
            return new CreateOrderResult(false, null, "An error occurred while creating the order.", [ex.Message]);
        }
    }
}
