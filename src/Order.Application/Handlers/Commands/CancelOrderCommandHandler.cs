using MediatR;
using Order.Application.Commands;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Repositories;

namespace Order.Application.Handlers.Commands;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;

    public CancelOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
    }

    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new CancelOrderResult(false, $"Order with ID '{request.OrderId}' not found.", null);
            }

            order.Cancel();
            await _orderRepository.UpdateAsync(order, cancellationToken);

            // Publish domain events
            foreach (var domainEvent in order.DomainEvents)
            {
                await _eventBus.PublishAsync(domainEvent, cancellationToken);
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
