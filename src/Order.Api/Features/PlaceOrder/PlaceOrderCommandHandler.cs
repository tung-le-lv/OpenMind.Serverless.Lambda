using MediatR;
using Order.Api.Application.Interfaces;
using Order.Api.Domain;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;

namespace Order.Api.Features.PlaceOrder;

public class PlaceOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    : IRequestHandler<PlaceOrderCommand, PlaceOrderResult>
{
    public async Task<PlaceOrderResult> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new PlaceOrderResult(false, $"Order '{request.OrderId}' not found.", null);
            }

            order.Place();

            await orderRepository.UpdateAsync(order, cancellationToken);

            foreach (var domainEvent in order.DomainEvents)
            {
                await eventBus.PublishAsync(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            return new PlaceOrderResult(true, "Order placed successfully.", null);
        }
        catch (DomainException ex)
        {
            return new PlaceOrderResult(false, ex.Message, null);
        }
        catch (Exception ex)
        {
            return new PlaceOrderResult(false, "An error occurred while placing the order.", [ex.Message]);
        }
    }
}
