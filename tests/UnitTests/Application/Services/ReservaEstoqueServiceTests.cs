using Application.Abstractions.Gateways;
using Application.Abstractions.Persistence;
using Application.Services;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Domain.Entities;
using Moq;

namespace UnitTests.Application.Services;

public class ReservaEstoqueServiceTests
{
    private readonly Mock<IItemEstoqueGateway> _itens = new();
    private readonly Mock<IReservaEstoqueGateway> _reservas = new();
    private readonly Mock<IEventoProcessadoGateway> _eventos = new();
    private readonly Mock<IOrdemDescartadaGateway> _descartes = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IEventPublisher> _publisher = new();
    private readonly List<object> _published = [];

    public ReservaEstoqueServiceTests()
    {
        _uow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(async (action, ct) => await action(ct));
        _uow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<(EstoqueReservadoData?, EstoqueEmFaltaData?)>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<(EstoqueReservadoData?, EstoqueEmFaltaData?)>>, CancellationToken>(
                async (action, ct) => await action(ct));
        _uow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<EstoqueReservadoData?>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<EstoqueReservadoData?>>, CancellationToken>(
                async (action, ct) => await action(ct));
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _publisher.Setup(p => p.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EstoqueReservadoData>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, EstoqueReservadoData, CancellationToken>((_, _, data, _) => _published.Add(data))
            .Returns(Task.CompletedTask);
        _publisher.Setup(p => p.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EstoqueEmFaltaData>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, EstoqueEmFaltaData, CancellationToken>((_, _, data, _) => _published.Add(data))
            .Returns(Task.CompletedTask);

        _eventos.Setup(e => e.ExisteEventIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _eventos.Setup(e => e.ExisteChaveNegocioAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _descartes.Setup(d => d.ExisteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _reservas.Setup(r => r.GetByOrdemEAprovacaoAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReservaEstoque?)null);
    }

    private ReservaEstoqueService Sut() => new(
        _itens.Object,
        _reservas.Object,
        _eventos.Object,
        _descartes.Object,
        _uow.Object,
        _publisher.Object);

    private static ItemEstoqueAggregateRoot Item(int id, decimal saldo, decimal reservado = 0) =>
        new() { Id = id, Codigo = $"C{id}", Nome = $"N{id}", Saldo = saldo, SaldoReservado = reservado, PrecoVenda = 10 };

    [Fact]
    public async Task ProcessarAprovacao_WhenCabe_PublishesEstoqueReservado_AndTrava()
    {
        // Arrange
        var item = Item(7, 10);
        _itens.Setup(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);
        var aprovadaEm = DateTimeOffset.Parse("2026-10-25T15:10:00Z");
        var data = new OrdemServicoAprovadaData
        {
            OrdemServicoId = 10,
            AprovadaEm = aprovadaEm,
            Itens = [new OrdemServicoAprovadaItem { ItemEstoqueId = 7, Quantidade = 2 }]
        };

        // Act
        await Sut().ProcessarAprovacaoAsync(Guid.NewGuid(), data);

        // Assert
        Assert.Equal(2, item.SaldoReservado);
        _reservas.Verify(r => r.AddAsync(It.Is<ReservaEstoque>(x => x.Status == ReservaEstoqueStatus.Reservada), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Single(_published);
        Assert.IsType<EstoqueReservadoData>(_published[0]);
    }

    [Fact]
    public async Task ProcessarAprovacao_RF01_1_SameItemLines_ShareSaldo()
    {
        // Arrange
        var item = Item(7, 3);
        _itens.Setup(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);
        var data = new OrdemServicoAprovadaData
        {
            OrdemServicoId = 10,
            AprovadaEm = DateTimeOffset.UtcNow,
            Itens =
            [
                new OrdemServicoAprovadaItem { ItemEstoqueId = 7, Quantidade = 2 },
                new OrdemServicoAprovadaItem { ItemEstoqueId = 7, Quantidade = 2 }
            ]
        };

        // Act
        await Sut().ProcessarAprovacaoAsync(Guid.NewGuid(), data);

        // Assert — 2+2=4 > 3 → falta, nada travado
        Assert.Equal(0, item.SaldoReservado);
        Assert.IsType<EstoqueEmFaltaData>(_published[0]);
        _reservas.Verify(r => r.AddAsync(It.Is<ReservaEstoque>(x => x.Status == ReservaEstoqueStatus.Pendente), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarAprovacao_WhenFalta_PublishesEstoqueEmFaltaOnce_AndKeepsPendente()
    {
        // Arrange
        var item = Item(7, 1);
        _itens.Setup(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);
        var data = new OrdemServicoAprovadaData
        {
            OrdemServicoId = 10,
            AprovadaEm = DateTimeOffset.UtcNow,
            Itens = [new OrdemServicoAprovadaItem { ItemEstoqueId = 7, Quantidade = 5 }]
        };

        // Act
        await Sut().ProcessarAprovacaoAsync(Guid.NewGuid(), data);

        // Assert
        Assert.Single(_published.OfType<EstoqueEmFaltaData>());
        Assert.Empty(_published.OfType<EstoqueReservadoData>());
        Assert.Equal(0, item.SaldoReservado);
    }

    [Fact]
    public async Task ProcessarAprovacao_WhenEventIdAlreadyProcessed_IsNoOp()
    {
        // Arrange
        _eventos.Setup(e => e.ExisteEventIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await Sut().ProcessarAprovacaoAsync(Guid.NewGuid(), new OrdemServicoAprovadaData
        {
            OrdemServicoId = 1,
            AprovadaEm = DateTimeOffset.UtcNow,
            Itens = []
        });

        // Assert
        Assert.Empty(_published);
        _itens.Verify(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarAprovacao_WhenBusinessKeyExists_IsNoOp()
    {
        // Arrange
        _eventos.Setup(e => e.ExisteChaveNegocioAsync(EventTypes.OrdemServicoAprovada, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await Sut().ProcessarAprovacaoAsync(Guid.NewGuid(), new OrdemServicoAprovadaData
        {
            OrdemServicoId = 1,
            AprovadaEm = DateTimeOffset.UtcNow,
            Itens = []
        });

        // Assert
        Assert.Empty(_published);
    }

    [Fact]
    public async Task ProcessarAprovacao_RF04_1_WhenOrdemDescartada_DoesNotReserve()
    {
        // Arrange
        _descartes.Setup(d => d.ExisteAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await Sut().ProcessarAprovacaoAsync(Guid.NewGuid(), new OrdemServicoAprovadaData
        {
            OrdemServicoId = 10,
            AprovadaEm = DateTimeOffset.UtcNow,
            Itens = [new OrdemServicoAprovadaItem { ItemEstoqueId = 7, Quantidade = 1 }]
        });

        // Assert
        Assert.Empty(_published);
        _itens.Verify(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventos.Verify(e => e.AddAsync(It.IsAny<EventoProcessado>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReavaliarPendencias_WhenNowFits_PublishesReservado_WithoutEmFalta()
    {
        // Arrange
        var item = Item(7, 10);
        var aprovadaEm = DateTimeOffset.Parse("2026-10-25T15:10:00Z");
        var pendente = ReservaEstoque.CriarPendente(10, aprovadaEm, [
            new ReservaEstoqueItem { ItemEstoqueId = 7, Quantidade = 2 }
        ]);
        pendente.Id = 1;

        _reservas.Setup(r => r.GetPendentesPorItemOrdenadasAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync([pendente]);
        _reservas.Setup(r => r.GetByOrdemEAprovacaoAsync(10, aprovadaEm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendente);
        _itens.Setup(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);

        // Act
        await Sut().ReavaliarPendenciasPorItemAsync(7);

        // Assert
        Assert.Equal(ReservaEstoqueStatus.Reservada, pendente.Status);
        Assert.Equal(2, item.SaldoReservado);
        Assert.Single(_published.OfType<EstoqueReservadoData>());
        Assert.Empty(_published.OfType<EstoqueEmFaltaData>());
    }

    [Fact]
    public async Task ReavaliarPendencias_WhenStillShort_DoesNotPublishEmFalta()
    {
        // Arrange
        var item = Item(7, 1);
        var aprovadaEm = DateTimeOffset.UtcNow;
        var pendente = ReservaEstoque.CriarPendente(10, aprovadaEm, [
            new ReservaEstoqueItem { ItemEstoqueId = 7, Quantidade = 5 }
        ]);
        _reservas.Setup(r => r.GetPendentesPorItemOrdenadasAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync([pendente]);
        _reservas.Setup(r => r.GetByOrdemEAprovacaoAsync(10, aprovadaEm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendente);
        _itens.Setup(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);

        // Act
        await Sut().ReavaliarPendenciasPorItemAsync(7);

        // Assert
        Assert.Equal(ReservaEstoqueStatus.Pendente, pendente.Status);
        Assert.Empty(_published);
    }

    [Fact]
    public async Task ProcessarFinalizacao_ConsumesReservedQuantities()
    {
        // Arrange
        var item = Item(7, 10, 2);
        var reserva = ReservaEstoque.CriarReservada(10, DateTimeOffset.UtcNow, [
            new ReservaEstoqueItem { ItemEstoqueId = 7, Quantidade = 2 }
        ]);
        _reservas.Setup(r => r.GetReservadaByOrdemServicoIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reserva);
        _itens.Setup(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);

        // Act
        await Sut().ProcessarFinalizacaoAsync(Guid.NewGuid(), new OrdemServicoFinalizadaData { OrdemServicoId = 10 });

        // Assert
        Assert.Equal(8, item.Saldo);
        Assert.Equal(0, item.SaldoReservado);
        Assert.Equal(ReservaEstoqueStatus.Consumida, reserva.Status);
    }

    [Fact]
    public async Task ProcessarDescarte_WhenReservada_LiberatesSaldo()
    {
        // Arrange
        var item = Item(7, 10, 2);
        var reserva = ReservaEstoque.CriarReservada(10, DateTimeOffset.UtcNow, [
            new ReservaEstoqueItem { ItemEstoqueId = 7, Quantidade = 2 }
        ]);
        _reservas.Setup(r => r.GetAtivaByOrdemServicoIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reserva);
        _itens.Setup(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);

        // Act
        await Sut().ProcessarDescarteAsync(Guid.NewGuid(), new OrdemServicoDescartadaData { OrdemServicoId = 10 });

        // Assert
        Assert.Equal(10, item.Saldo);
        Assert.Equal(0, item.SaldoReservado);
        Assert.Equal(ReservaEstoqueStatus.Liberada, reserva.Status);
        _descartes.Verify(d => d.AddAsync(It.Is<OrdemDescartada>(o => o.OrdemServicoId == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarDescarte_WhenPendente_RemovesFromQueueWithoutTouchingSaldo()
    {
        // Arrange
        var reserva = ReservaEstoque.CriarPendente(10, DateTimeOffset.UtcNow, [
            new ReservaEstoqueItem { ItemEstoqueId = 7, Quantidade = 2 }
        ]);
        _reservas.Setup(r => r.GetAtivaByOrdemServicoIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reserva);

        // Act
        await Sut().ProcessarDescarteAsync(Guid.NewGuid(), new OrdemServicoDescartadaData { OrdemServicoId = 10 });

        // Assert
        Assert.Equal(ReservaEstoqueStatus.Liberada, reserva.Status);
        _itens.Verify(g => g.GetEBloquearItensAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarFinalizacao_WhenAlreadyProcessed_IsNoOp()
    {
        // Arrange
        _eventos.Setup(e => e.ExisteEventIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await Sut().ProcessarFinalizacaoAsync(Guid.NewGuid(), new OrdemServicoFinalizadaData { OrdemServicoId = 10 });

        // Assert
        _reservas.Verify(r => r.GetReservadaByOrdemServicoIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
