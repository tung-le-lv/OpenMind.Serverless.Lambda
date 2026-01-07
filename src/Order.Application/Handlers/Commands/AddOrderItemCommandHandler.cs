using MediatR;
using Order.Application.Commands;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Repositories;

namespace Order.Application.Handlers.Commands;

public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand, AddOrderItemResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;

    public AddOrderItemCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
    }

    public async Task<AddOrderItemResult> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new AddOrderItemResult(false, $"Order with ID '{request.OrderId}' not found.", null);
            }

            order.AddItem(request.ProductId, request.ProductName, request.Quantity, request.UnitPrice);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            // Publish domain events
            foreach (var domainEvent in order.DomainEvents)
            {
                await _eventBus.PublishAsync(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            return new AddOrderItemResult(true, "Item added to order successfully.", null);
        }
        catch (DomainException ex)
        {
            return new AddOrderItemResult(false, "Failed to add item.", [ex.Message]);
        }
        catch (Exception ex)
        {
            return new AddOrderItemResult(false, "An error occurred while adding the item.", [ex.Message]);
        }
    }
}
