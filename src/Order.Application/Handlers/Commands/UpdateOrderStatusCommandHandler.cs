using MediatR;
using Order.Application.Commands;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Repositories;

namespace Order.Application.Handlers.Commands;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
    }

    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new UpdateOrderStatusResult(false, $"Order with ID '{request.OrderId}' not found.", null);
            }

            order.UpdateStatus(request.NewStatus);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            // Publish domain events
            foreach (var domainEvent in order.DomainEvents)
            {
                await _eventBus.PublishAsync(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            return new UpdateOrderStatusResult(true, "Order status updated successfully.", null);
        }
        catch (DomainException ex)
        {
            return new UpdateOrderStatusResult(false, "Status update failed.", [ex.Message]);
        }
        catch (Exception ex)
        {
            return new UpdateOrderStatusResult(false, "An error occurred while updating the order status.", [ex.Message]);
        }
    }
}
