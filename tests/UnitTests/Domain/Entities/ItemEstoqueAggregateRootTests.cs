using Domain.Entities;

namespace UnitTests.Domain.Entities;

public class ItemEstoqueAggregateRootTests
{
    [Fact]
    public void RegistrarEntradaEstoque_IncrementsSaldo()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 10, SaldoReservado = 2 };

        // Act
        item.RegistrarEntradaEstoque(5);

        // Assert
        Assert.Equal(15, item.Saldo);
        Assert.Equal(13, item.SaldoDisponivel);
    }

    [Fact]
    public void TravarEstoque_IncrementsSaldoReservado()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 10, SaldoReservado = 1 };

        // Act
        item.TravarEstoque(4);

        // Assert
        Assert.Equal(5, item.SaldoReservado);
        Assert.Equal(5, item.SaldoDisponivel);
    }

    [Fact]
    public void TravarEstoque_WhenInsufficient_Throws()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 5, SaldoReservado = 3 };

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => item.TravarEstoque(3));
    }

    [Fact]
    public void LiberarReserva_DecrementsSaldoReservado()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 10, SaldoReservado = 4 };

        // Act
        item.LiberarReserva(3);

        // Assert
        Assert.Equal(1, item.SaldoReservado);
        Assert.Equal(10, item.Saldo);
    }

    [Fact]
    public void ConfirmarUtilizacao_DecrementsSaldoAndReservado()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 10, SaldoReservado = 4 };

        // Act
        item.ConfirmarUtilizacao(3);

        // Assert
        Assert.Equal(7, item.Saldo);
        Assert.Equal(1, item.SaldoReservado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RegistrarEntradaEstoque_WhenInvalid_Throws(decimal q)
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 1 };

        // Act / Assert
        Assert.Throws<ArgumentException>(() => item.RegistrarEntradaEstoque(q));
    }

    [Fact]
    public void LiberarReserva_WhenExceeds_Throws()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 5, SaldoReservado = 1 };

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => item.LiberarReserva(2));
    }

    [Fact]
    public void ConfirmarUtilizacao_WhenExceedsReservado_Throws()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 5, SaldoReservado = 1 };

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => item.ConfirmarUtilizacao(2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TravarEstoque_WhenInvalidQty_Throws(decimal q)
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 5 };

        // Act / Assert
        Assert.Throws<ArgumentException>(() => item.TravarEstoque(q));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void LiberarReserva_WhenInvalidQty_Throws(decimal q)
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 5, SaldoReservado = 2 };

        // Act / Assert
        Assert.Throws<ArgumentException>(() => item.LiberarReserva(q));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConfirmarUtilizacao_WhenInvalidQty_Throws(decimal q)
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 5, SaldoReservado = 2 };

        // Act / Assert
        Assert.Throws<ArgumentException>(() => item.ConfirmarUtilizacao(q));
    }

    [Fact]
    public void ConfirmarUtilizacao_WhenExceedsSaldo_Throws()
    {
        // Arrange
        var item = new ItemEstoqueAggregateRoot { Saldo = 1, SaldoReservado = 5 };

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => item.ConfirmarUtilizacao(2));
    }
}
