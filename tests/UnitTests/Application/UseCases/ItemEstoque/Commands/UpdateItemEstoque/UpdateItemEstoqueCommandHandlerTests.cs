using Application.Abstractions.Gateways;
using Application.UseCases.ItemEstoque.Commands.UpdateItemEstoque;
using Domain.Entities;
using Domain.Exceptions;
using Moq;

namespace UnitTests.Application.UseCases.ItemEstoque.Commands.UpdateItemEstoque;

public class UpdateItemEstoqueCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNotFound_Throws()
    {
        // Arrange
        var gw = new Mock<IItemEstoqueGateway>();
        gw.Setup(g => g.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((ItemEstoqueAggregateRoot?)null);

        // Act / Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            new UpdateItemEstoqueCommandHandler(gw.Object).Handle(new UpdateItemEstoqueCommand { Id = 1, Codigo = "A", Nome = "N", PrecoVenda = 1 }, default));
    }

    [Fact]
    public async Task Handle_WhenOk_Updates()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Id = 1, Codigo = "A", Nome = "Old", PrecoVenda = 1, Saldo = 1 };
        var gw = new Mock<IItemEstoqueGateway>();
        gw.Setup(g => g.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        gw.Setup(g => g.GetByCodigoAsync("B", It.IsAny<CancellationToken>())).ReturnsAsync((ItemEstoqueAggregateRoot?)null);

        // Act
        var result = await new UpdateItemEstoqueCommandHandler(gw.Object).Handle(new UpdateItemEstoqueCommand
        {
            Id = 1, Codigo = "B", Nome = "New", Tipo = ItemTipo.Insumo, UnidadeMedida = UnidadeMedida.Kg, PrecoVenda = 5
        }, default);

        // Assert
        Assert.Equal("B", result.Codigo);
        Assert.Equal("New", result.Nome);
        gw.Verify(g => g.UpdateAsync(item, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCodigoTaken_Throws()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Id = 1, Codigo = "A" };
        var gw = new Mock<IItemEstoqueGateway>();
        gw.Setup(g => g.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        gw.Setup(g => g.GetByCodigoAsync("B", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemEstoqueAggregateRoot { Id = 2, Codigo = "B" });

        // Act / Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            new UpdateItemEstoqueCommandHandler(gw.Object).Handle(new UpdateItemEstoqueCommand { Id = 1, Codigo = "B", Nome = "N", PrecoVenda = 1 }, default));
    }
}
