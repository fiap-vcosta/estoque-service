using Application.Abstractions.Gateways;
using Application.Abstractions.Persistence;
using Application.Services;
using Application.UseCases.ItemEstoque.Commands.RegistrarEntradaEstoque;
using BuildingBlocks.Messaging;
using Domain.Entities;
using Domain.Exceptions;
using Moq;

namespace UnitTests.Application.UseCases.ItemEstoque.Commands.RegistrarEntradaEstoque;

public class RegistrarEntradaEstoqueCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNotFound_Throws()
    {
        // Arrange
        var gw = new Mock<IItemEstoqueGateway>();
        var service = new ReservaEstoqueService(gw.Object, Mock.Of<IReservaEstoqueGateway>(), Mock.Of<IEventoProcessadoGateway>(),
            Mock.Of<IOrdemDescartadaGateway>(), Mock.Of<IUnitOfWork>(), Mock.Of<IEventPublisher>());
        gw.Setup(g => g.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((ItemEstoqueAggregateRoot?)null);

        // Act / Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            new RegistrarEntradaEstoqueCommandHandler(gw.Object, service).Handle(
                new RegistrarEntradaEstoqueCommand { IdItemEstoque = 1, QuantidadeRecebida = 1 }, default));
    }

    [Fact]
    public async Task Handle_WhenOk_IncrementsAndReavalia()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Id = 1, Saldo = 1, Codigo = "A", Nome = "N", PrecoVenda = 1 };
        var gw = new Mock<IItemEstoqueGateway>();
        var reservas = new Mock<IReservaEstoqueGateway>();
        reservas.Setup(r => r.GetPendentesPorItemOrdenadasAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ReservaEstoque>());
        var uow = new Mock<IUnitOfWork>();
        var service = new ReservaEstoqueService(gw.Object, reservas.Object, Mock.Of<IEventoProcessadoGateway>(),
            Mock.Of<IOrdemDescartadaGateway>(), uow.Object, Mock.Of<IEventPublisher>());
        gw.Setup(g => g.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        // Act
        var result = await new RegistrarEntradaEstoqueCommandHandler(gw.Object, service).Handle(
            new RegistrarEntradaEstoqueCommand { IdItemEstoque = 1, QuantidadeRecebida = 4 }, default);

        // Assert
        Assert.Equal(5, result.Saldo);
    }

    [Fact]
    public async Task Handle_WhenQuantidadeInvalid_Throws()
    {
        // Arrange
        var gw = new Mock<IItemEstoqueGateway>();
        var service = new ReservaEstoqueService(gw.Object, Mock.Of<IReservaEstoqueGateway>(), Mock.Of<IEventoProcessadoGateway>(),
            Mock.Of<IOrdemDescartadaGateway>(), Mock.Of<IUnitOfWork>(), Mock.Of<IEventPublisher>());

        // Act / Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            new RegistrarEntradaEstoqueCommandHandler(gw.Object, service).Handle(
                new RegistrarEntradaEstoqueCommand { IdItemEstoque = 1, QuantidadeRecebida = 0 }, default));
    }
}
