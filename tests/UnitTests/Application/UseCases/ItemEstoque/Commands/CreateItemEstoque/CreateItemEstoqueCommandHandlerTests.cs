using Application.Abstractions.Gateways;
using Application.UseCases.ItemEstoque.Commands.CreateItemEstoque;
using Domain.Entities;
using Domain.Exceptions;
using Moq;

namespace UnitTests.Application.UseCases.ItemEstoque.Commands.CreateItemEstoque;

public class CreateItemEstoqueCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCodigoExists_Throws()
    {
        // Arrange
        var gateway = new Mock<IItemEstoqueGateway>();
        gateway.Setup(g => g.GetByCodigoAsync("X", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemEstoqueAggregateRoot { Id = 1, Codigo = "X" });
        var handler = new CreateItemEstoqueCommandHandler(gateway.Object);

        // Act / Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new CreateItemEstoqueCommand
        {
            Codigo = "X", Nome = "n", PrecoVenda = 1, Saldo = 1
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenOk_Creates()
    {
        // Arrange
        var gateway = new Mock<IItemEstoqueGateway>();
        gateway.Setup(g => g.GetByCodigoAsync("Y", It.IsAny<CancellationToken>())).ReturnsAsync((ItemEstoqueAggregateRoot?)null);
        gateway.Setup(g => g.CreateAsync(It.IsAny<ItemEstoqueAggregateRoot>(), It.IsAny<CancellationToken>()))
            .Callback<ItemEstoqueAggregateRoot, CancellationToken>((i, _) => i.Id = 9)
            .Returns(Task.CompletedTask);

        // Act
        var result = await new CreateItemEstoqueCommandHandler(gateway.Object).Handle(new CreateItemEstoqueCommand
        {
            Codigo = "Y", Nome = "n", Tipo = ItemTipo.Peca, UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 2, Saldo = 3, SaldoReservado = 1
        }, default);

        // Assert
        Assert.Equal(9, result.Id);
        Assert.Equal("Y", result.Codigo);
    }

    [Fact]
    public async Task Handle_WhenPrecoInvalid_Throws()
    {
        // Arrange
        var gateway = new Mock<IItemEstoqueGateway>();
        gateway.Setup(g => g.GetByCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((ItemEstoqueAggregateRoot?)null);

        // Act / Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            new CreateItemEstoqueCommandHandler(gateway.Object).Handle(new CreateItemEstoqueCommand
            {
                Codigo = "Y", Nome = "n", PrecoVenda = 0, Saldo = 1
            }, default));
    }
}
