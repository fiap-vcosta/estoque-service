using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure.Database.Gateways;

public class ReservaEstoqueGatewayTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ReservaEstoqueGateway _gateway;
    private readonly DateTimeOffset _aprovadaEm = DateTimeOffset.Parse("2026-10-25T15:10:00Z");

    public ReservaEstoqueGatewayTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _gateway = new ReservaEstoqueGateway(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAndQuery_Work()
    {
        // Arrange
        var reserva = ReservaEstoque.CriarPendente(Guid.Parse("00000000-0000-0000-0000-000000000010"), _aprovadaEm, [
            new ReservaEstoqueItem { ItemEstoqueId = 7, Quantidade = 2 }
        ]);

        // Act
        await _gateway.AddAsync(reserva);
        var byKey = await _gateway.GetByOrdemEAprovacaoAsync(Guid.Parse("00000000-0000-0000-0000-000000000010"), _aprovadaEm);
        var pendentes = await _gateway.GetPendentesPorItemOrdenadasAsync(7);
        var ativa = await _gateway.GetAtivaByOrdemServicoIdAsync(Guid.Parse("00000000-0000-0000-0000-000000000010"));

        // Assert
        Assert.NotNull(byKey);
        Assert.Single(pendentes);
        Assert.NotNull(ativa);

        reserva.MarcarReservada();
        await _gateway.UpdateAsync(reserva);
        var reservada = await _gateway.GetReservadaByOrdemServicoIdAsync(Guid.Parse("00000000-0000-0000-0000-000000000010"));
        Assert.NotNull(reservada);
    }

    [Fact]
    public async Task Queries_WhenEmpty_ReturnNullOrEmpty()
    {
        // Arrange / Act
        var byKey = await _gateway.GetByOrdemEAprovacaoAsync(Guid.Parse("00000000-0000-0000-0000-000000000001"), _aprovadaEm);
        var reservada = await _gateway.GetReservadaByOrdemServicoIdAsync(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var ativa = await _gateway.GetAtivaByOrdemServicoIdAsync(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var pendentes = await _gateway.GetPendentesPorItemOrdenadasAsync(1);

        // Assert
        Assert.Null(byKey);
        Assert.Null(reservada);
        Assert.Null(ativa);
        Assert.Empty(pendentes);
    }
}
