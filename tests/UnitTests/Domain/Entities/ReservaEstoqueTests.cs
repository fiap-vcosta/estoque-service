using Domain.Entities;

namespace UnitTests.Domain.Entities;

public class ReservaEstoqueTests
{
    [Fact]
    public void MarcarReservada_FromPendente_Works()
    {
        // Arrange
        var reserva = ReservaEstoque.CriarPendente(Guid.Parse("00000000-0000-0000-0000-000000000001"), DateTimeOffset.UtcNow, [
            new ReservaEstoqueItem { ItemEstoqueId = 7, Quantidade = 2 }
        ]);

        // Act
        reserva.MarcarReservada();

        // Assert
        Assert.Equal(ReservaEstoqueStatus.Reservada, reserva.Status);
    }

    [Fact]
    public void MarcarConsumida_FromPendente_Throws()
    {
        // Arrange
        var reserva = ReservaEstoque.CriarPendente(Guid.Parse("00000000-0000-0000-0000-000000000001"), DateTimeOffset.UtcNow, []);

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => reserva.MarcarConsumida());
    }

    [Fact]
    public void MarcarReservada_FromReservada_Throws()
    {
        // Arrange
        var reserva = ReservaEstoque.CriarReservada(Guid.Parse("00000000-0000-0000-0000-000000000001"), DateTimeOffset.UtcNow, [
            new ReservaEstoqueItem { ItemEstoqueId = 7, Quantidade = 1 }
        ]);

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => reserva.MarcarReservada());
    }

    [Fact]
    public void MarcarLiberada_FromConsumida_Throws()
    {
        // Arrange
        var reserva = ReservaEstoque.CriarReservada(Guid.Parse("00000000-0000-0000-0000-000000000001"), DateTimeOffset.UtcNow, [
            new ReservaEstoqueItem { ItemEstoqueId = 7, Quantidade = 1 }
        ]);
        reserva.MarcarConsumida();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => reserva.MarcarLiberada());
    }

    [Fact]
    public void ReservaEstoqueItem_HoldsIds()
    {
        // Arrange / Act
        var item = new ReservaEstoqueItem { Id = 3, ReservaEstoqueId = 9, ItemEstoqueId = 7, Quantidade = 2 };

        // Assert
        Assert.Equal(3, item.Id);
        Assert.Equal(9, item.ReservaEstoqueId);
        Assert.Equal(7, item.ItemEstoqueId);
        Assert.Equal(2, item.Quantidade);
    }
}
