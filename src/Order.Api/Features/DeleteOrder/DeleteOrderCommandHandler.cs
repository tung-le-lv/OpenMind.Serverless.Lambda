using MediatR;
using Order.Api.Domain.Repositories;

namespace Order.Api.Features.DeleteOrder;

public class DeleteOrderCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<DeleteOrderCommand, DeleteOrderResult>
{
    public async Task<DeleteOrderResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await orderRepository.DeleteAsync(request.OrderId, cancellationToken);
            return new DeleteOrderResult(true, "Order deleted successfully.");
        }
        catch (Exception ex)
        {
            return new DeleteOrderResult(false, $"An error occurred while deleting the order: {ex.Message}");
        }
    }
}
