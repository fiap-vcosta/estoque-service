using Application.Abstractions.Gateways;
using Application.UseCases.ItemEstoque.Queries.GetItemEstoqueById;
using Domain.Entities;
using Moq;

namespace UnitTests.Application.UseCases.ItemEstoque.Queries.GetItemEstoqueById;

public class GetItemEstoqueByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenMissing_ReturnsNull()
    {
        // Arrange
        var gw = new Mock<IItemEstoqueGateway>();
        gw.Setup(g => g.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((ItemEstoqueAggregateRoot?)null);

        // Act
        var result = await new GetItemEstoqueByIdQueryHandler(gw.Object).Handle(new GetItemEstoqueByIdQuery { Id = 1 }, default);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenExists_ReturnsResponse()
    {
        // Arrange
        var gw = new Mock<IItemEstoqueGateway>();
        gw.Setup(g => g.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemEstoqueAggregateRoot { Id = 1, Codigo = "A", Nome = "N", PrecoVenda = 1 });

        // Act
        var result = await new GetItemEstoqueByIdQueryHandler(gw.Object).Handle(new GetItemEstoqueByIdQuery { Id = 1 }, default);

        // Assert
        Assert.Equal("A", result!.Codigo);
    }
}
