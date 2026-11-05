using Application.Abstractions.Gateways;
using Application.UseCases.ItemEstoque.Commands.DeleteItemEstoque;
using Domain.Entities;
using Domain.Exceptions;
using Moq;

namespace UnitTests.Application.UseCases.ItemEstoque.Commands.DeleteItemEstoque;

public class DeleteItemEstoqueCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNotFound_Throws()
    {
        // Arrange
        var gw = new Mock<IItemEstoqueGateway>();
        gw.Setup(g => g.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((ItemEstoqueAggregateRoot?)null);

        // Act / Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            new DeleteItemEstoqueCommandHandler(gw.Object).Handle(new DeleteItemEstoqueCommand { Id = 1 }, default));
    }

    [Fact]
    public async Task Handle_WhenExists_Deletes()
    {
        // Arrange
        var gw = new Mock<IItemEstoqueGateway>();
        gw.Setup(g => g.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemEstoqueAggregateRoot { Id = 1 });

        // Act
        await new DeleteItemEstoqueCommandHandler(gw.Object).Handle(new DeleteItemEstoqueCommand { Id = 1 }, default);

        // Assert
        gw.Verify(g => g.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
