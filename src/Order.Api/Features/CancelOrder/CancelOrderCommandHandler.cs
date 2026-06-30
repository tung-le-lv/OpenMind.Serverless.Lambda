using MediatR;
using Order.Api.Domain.Entities;
using Order.Api.Application.Interfaces;
using Order.Api.Domain;
using Order.Api.Domain.Repositories;

namespace Order.Api.Features.CancelOrder;

public class CancelOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    : IRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new CancelOrderResult(false, $"Order with ID '{request.OrderId}' not found.", null);
            }

            order.Cancel();
            await orderRepository.UpdateAsync(order, cancellationToken);

            foreach (var domainEvent in order.DomainEvents)
            {
                await eventBus.PublishAsync(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            return new CancelOrderResult(true, "Order cancelled successfully.", null);
        }
        catch (DomainException ex)
        {
            return new CancelOrderResult(false, "Cancellation failed.", [ex.Message]);
        }
        catch (Exception ex)
        {
            return new CancelOrderResult(false, "An error occurred while cancelling the order.", [ex.Message]);
        }
    }
}
