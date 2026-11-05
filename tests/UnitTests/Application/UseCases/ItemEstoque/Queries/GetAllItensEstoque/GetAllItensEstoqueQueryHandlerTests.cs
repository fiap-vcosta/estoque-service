using Application.Abstractions.Gateways;
using Application.UseCases.ItemEstoque.Queries.GetAllItensEstoque;
using Domain.Entities;
using Moq;

namespace UnitTests.Application.UseCases.ItemEstoque.Queries.GetAllItensEstoque;

public class GetAllItensEstoqueQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMappedList()
    {
        // Arrange
        var gw = new Mock<IItemEstoqueGateway>();
        gw.Setup(g => g.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ItemEstoqueAggregateRoot { Id = 1, Codigo = "A", Nome = "N", PrecoVenda = 1 }]);

        // Act
        var result = await new GetAllItensEstoqueQueryHandler(gw.Object).Handle(new GetAllItensEstoqueQuery(), default);

        // Assert
        Assert.Single(result);
    }
}
