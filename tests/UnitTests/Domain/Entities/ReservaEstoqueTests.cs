using Domain.Entities;

namespace UnitTests.Domain.Entities;

public class ReservaEstoqueTests
{
    [Fact]
    public void MarcarReservada_FromPendente_Works()
    {
        // Arrange
        var reserva = ReservaEstoque.CriarPendente(1, DateTimeOffset.UtcNow, [
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
        var reserva = ReservaEstoque.CriarPendente(1, DateTimeOffset.UtcNow, []);

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => reserva.MarcarConsumida());
    }
}
