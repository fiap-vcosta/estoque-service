using Application.Services;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Domain.Entities;
using Infrastructure.Database;
using IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace IntegrationTests.Reservas;

[Collection(nameof(IntegrationFixture))]
public class ReservaEstoqueIntegrationTests
{
    private readonly CustomWebApplicationFactory _factory;

    public ReservaEstoqueIntegrationTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Aprovacao_Reservada_ThenFinalizacao_Consumes()
    {
        // Arrange
        var itemId = await CreateItemAsync("INT-RES-1", saldo: 10);
        await using var scope = _factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ReservaEstoqueService>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var transport = scope.ServiceProvider.GetRequiredService<InMemoryPubSubTransport>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aprovadaEm = DateTimeOffset.Parse("2026-10-25T15:10:00Z");

        // Act
        await service.ProcessarAprovacaoAsync(Guid.NewGuid(), new OrdemServicoAprovadaData
        {
            OrdemServicoId = 100,
            AprovadaEm = aprovadaEm,
            Itens = [new OrdemServicoAprovadaItem { ItemEstoqueId = itemId, Quantidade = 3 }]
        });

        // Assert
        var item = await db.ItensEstoque.FindAsync(itemId);
        Assert.Equal(3, item!.SaldoReservado);
        Assert.NotEmpty(transport.GetPublished(TopicIds.EstoqueReservado));

        await service.ProcessarFinalizacaoAsync(Guid.NewGuid(), new OrdemServicoFinalizadaData { OrdemServicoId = 100 });
        await db.Entry(item).ReloadAsync();
        Assert.Equal(7, item.Saldo);
        Assert.Equal(0, item.SaldoReservado);
        Assert.Equal(ReservaEstoqueStatus.Consumida, db.ReservasEstoque.Single(r => r.OrdemServicoId == 100).Status);
        _ = publisher;
    }

    [Fact]
    public async Task Aprovacao_EmFalta_ThenEntrada_ReavaliaToReservado()
    {
        // Arrange
        var itemId = await CreateItemAsync("INT-FALTA-1", saldo: 1);
        await using var scope = _factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ReservaEstoqueService>();
        var transport = scope.ServiceProvider.GetRequiredService<InMemoryPubSubTransport>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aprovadaEm = DateTimeOffset.Parse("2026-10-25T16:00:00Z");

        await service.ProcessarAprovacaoAsync(Guid.NewGuid(), new OrdemServicoAprovadaData
        {
            OrdemServicoId = 200,
            AprovadaEm = aprovadaEm,
            Itens = [new OrdemServicoAprovadaItem { ItemEstoqueId = itemId, Quantidade = 5 }]
        });
        Assert.NotEmpty(transport.GetPublished(TopicIds.EstoqueEmFalta));
        Assert.Equal(ReservaEstoqueStatus.Pendente, db.ReservasEstoque.Single(r => r.OrdemServicoId == 200).Status);

        // Act — entrada via HTTP (reavalia)
        var client = _factory.CreateClient();
        AuthHelper.AuthenticateAsStaff(client);
        var entrada = await client.PostAsJsonAsync($"/estoque/itens/{itemId}/registrar-entrada", new { quantidade = 10 });
        entrada.EnsureSuccessStatusCode();

        // Assert
        await db.Entry(db.ReservasEstoque.Single(r => r.OrdemServicoId == 200)).ReloadAsync();
        Assert.Equal(ReservaEstoqueStatus.Reservada, db.ReservasEstoque.Single(r => r.OrdemServicoId == 200).Status);
        Assert.NotEmpty(transport.GetPublished(TopicIds.EstoqueReservado));
    }

    [Fact]
    public async Task Descarte_AfterReserva_Liberates_AndBlocksLaterAprovacao()
    {
        // Arrange
        var itemId = await CreateItemAsync("INT-DESC-1", saldo: 10);
        await using var scope = _factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ReservaEstoqueService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aprovadaEm = DateTimeOffset.Parse("2026-10-25T17:00:00Z");

        await service.ProcessarAprovacaoAsync(Guid.NewGuid(), new OrdemServicoAprovadaData
        {
            OrdemServicoId = 300,
            AprovadaEm = aprovadaEm,
            Itens = [new OrdemServicoAprovadaItem { ItemEstoqueId = itemId, Quantidade = 2 }]
        });

        await service.ProcessarDescarteAsync(Guid.NewGuid(), new OrdemServicoDescartadaData { OrdemServicoId = 300 });
        var item = await db.ItensEstoque.FindAsync(itemId);
        Assert.Equal(0, item!.SaldoReservado);

        // Act — RF04.1
        await service.ProcessarAprovacaoAsync(Guid.NewGuid(), new OrdemServicoAprovadaData
        {
            OrdemServicoId = 300,
            AprovadaEm = DateTimeOffset.Parse("2026-10-25T18:00:00Z"),
            Itens = [new OrdemServicoAprovadaItem { ItemEstoqueId = itemId, Quantidade = 1 }]
        });

        // Assert
        await db.Entry(item).ReloadAsync();
        Assert.Equal(0, item.SaldoReservado);
        Assert.True(db.OrdensDescartadas.Any(o => o.OrdemServicoId == 300));
    }

    private async Task<int> CreateItemAsync(string codigo, decimal saldo)
    {
        var client = _factory.CreateClient();
        AuthHelper.AuthenticateAsStaff(client);
        var response = await client.PostAsJsonAsync("/estoque/itens", new
        {
            codigo,
            tipo = "Peca",
            nome = codigo,
            unidadeMedida = "Unidade",
            precoVenda = 10,
            saldo,
            saldoReservado = 0
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        return json.GetProperty("id").GetInt32();
    }
}
