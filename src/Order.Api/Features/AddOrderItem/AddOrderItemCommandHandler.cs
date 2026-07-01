using MediatR;
using Order.Api.Domain.Entities;
using Order.Api.Domain;
using Order.Api.Domain.Repositories;
using Order.Api.Shared.Application.Interfaces;

namespace Order.Api.Features.AddOrderItem;

public class AddOrderItemCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    : IRequestHandler<AddOrderItemCommand, AddOrderItemResult>
{
    public async Task<AddOrderItemResult> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new AddOrderItemResult(false, $"Order with ID '{request.OrderId}' not found.", null);
            }

            order.AddItem(request.ProductId, request.ProductName, request.Quantity, request.UnitPrice);
            await orderRepository.UpdateAsync(order, cancellationToken);

            foreach (var domainEvent in order.DomainEvents)
            {
                await eventBus.PublishAsync(domainEvent, cancellationToken);
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
