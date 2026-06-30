using MediatR;
using Order.Api.Domain.Entities;
using Order.Api.Application.Interfaces;
using Order.Api.Domain;
using Order.Api.Domain.Repositories;

namespace Order.Api.Features.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new UpdateOrderStatusResult(false, $"Order with ID '{request.OrderId}' not found.", null);
            }

            order.UpdateStatus(request.NewStatus);
            await orderRepository.UpdateAsync(order, cancellationToken);

            foreach (var domainEvent in order.DomainEvents)
            {
                await eventBus.PublishAsync(domainEvent, cancellationToken);
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
