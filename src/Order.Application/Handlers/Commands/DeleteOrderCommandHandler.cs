using MediatR;
using Order.Application.Commands;
using Order.Domain.Repositories;

namespace Order.Application.Handlers.Commands;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, DeleteOrderResult>
{
    private readonly IOrderRepository _orderRepository;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<DeleteOrderResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _orderRepository.DeleteAsync(request.OrderId, cancellationToken);
            return new DeleteOrderResult(true, "Order deleted successfully.");
        }
        catch (Exception ex)
        {
            return new DeleteOrderResult(false, $"An error occurred while deleting the order: {ex.Message}");
        }
    }
}
