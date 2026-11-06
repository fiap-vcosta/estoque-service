using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure.Database.Gateways;

public class ItemEstoqueGatewayTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ItemEstoqueGateway _gateway;

    public ItemEstoqueGatewayTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _gateway = new ItemEstoqueGateway(_context);
        _context.ItensEstoque.Add(new ItemEstoqueAggregateRoot
        {
            Id = 1, Codigo = "A", Nome = "N", Tipo = ItemTipo.Peca, UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 1, Saldo = 5, SaldoReservado = 1
        });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsItem()
    {
        // Arrange / Act
        var item = await _gateway.GetByIdAsync(1);

        // Assert
        Assert.NotNull(item);
        Assert.Equal("A", item.Codigo);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsItems()
    {
        // Arrange / Act
        var items = await _gateway.GetAllAsync();

        // Assert
        Assert.Single(items);
    }

    [Fact]
    public async Task GetByCodigoAsync_ReturnsItem()
    {
        // Arrange / Act
        var item = await _gateway.GetByCodigoAsync("A");

        // Assert
        Assert.NotNull(item);
    }

    [Fact]
    public async Task CreateUpdateDelete_Work()
    {
        // Arrange
        var created = new ItemEstoqueAggregateRoot
        {
            Codigo = "B", Nome = "B", Tipo = ItemTipo.Insumo, UnidadeMedida = UnidadeMedida.Kg,
            PrecoVenda = 2, Saldo = 1
        };

        // Act
        await _gateway.CreateAsync(created);
        created.Nome = "BB";
        await _gateway.UpdateAsync(created);
        await _gateway.DeleteAsync(created.Id);

        // Assert
        Assert.Null(await _gateway.GetByCodigoAsync("B"));
    }

    [Fact]
    public async Task GetEBloquearItensAsync_WhenEmpty_ReturnsEmpty()
    {
        // Arrange / Act
        var result = await _gateway.GetEBloquearItensAsync([]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        // Arrange / Act
        var item = await _gateway.GetByIdAsync(999);

        // Assert
        Assert.Null(item);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_IsNoOp()
    {
        // Arrange / Act
        await _gateway.DeleteAsync(999);

        // Assert
        Assert.Single(await _gateway.GetAllAsync());
    }
}
