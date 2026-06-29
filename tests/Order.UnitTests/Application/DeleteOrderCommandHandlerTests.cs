using FluentAssertions;
using Moq;
using Order.Api.Domain.Repositories;
using Order.Api.Features.DeleteOrder;
using Xunit;

namespace Order.UnitTests.Application;

public class DeleteOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly DeleteOrderHandler _handler;

    public DeleteOrderCommandHandlerTests() =>
        _handler = new DeleteOrderHandler(_repo.Object);

    [Fact]
    public async Task Handle_WhenOrderExists_ShouldDeleteOrderSuccessfully()
    {
        _repo.Setup(r => r.DeleteAsync("order-1", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new DeleteOrderCommand("order-1"), CancellationToken.None);

        result.Success.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync("order-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeletionFails_ShouldReturnError()
    {
        _repo.Setup(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB error"));

        var result = await _handler.Handle(new DeleteOrderCommand("order-1"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("DB error");
    }
}
