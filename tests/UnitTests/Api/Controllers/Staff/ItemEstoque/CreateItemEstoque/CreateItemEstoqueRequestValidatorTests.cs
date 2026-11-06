using Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;
using Domain.Entities;

namespace UnitTests.Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;

public class CreateItemEstoqueRequestValidatorTests
{
    private readonly CreateItemEstoqueRequestValidator _sut = new();

    [Fact]
    public void Validate_WhenEmpty_HasErrors()
    {
        // Arrange / Act
        var result = _sut.Validate(new CreateItemEstoqueRequest
        {
            Tipo = default,
            UnidadeMedida = default,
            PrecoVenda = default,
            Saldo = default,
            SaldoReservado = default
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_WhenOk_IsValid()
    {
        // Arrange / Act
        var result = _sut.Validate(new CreateItemEstoqueRequest
        {
            Codigo = "A",
            Nome = "N",
            Tipo = ItemTipo.Peca,
            UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 1,
            Saldo = 2,
            SaldoReservado = 1
        });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenReservadoMaiorQueSaldo_HasError()
    {
        // Arrange / Act
        var result = _sut.Validate(new CreateItemEstoqueRequest
        {
            Codigo = "A",
            Nome = "N",
            Tipo = ItemTipo.Peca,
            UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 1,
            Saldo = 1,
            SaldoReservado = 2
        });

        // Assert
        Assert.Contains(result.Errors, e => e.Contains("reservado"));
    }

    [Fact]
    public void Validate_WhenSaldoNegativo_HasError()
    {
        // Arrange / Act
        var result = _sut.Validate(new CreateItemEstoqueRequest
        {
            Codigo = "A",
            Nome = "N",
            Tipo = ItemTipo.Peca,
            UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 1,
            Saldo = -1,
            SaldoReservado = 0
        });

        // Assert
        Assert.Contains(result.Errors, e => e.Contains("Saldo não pode ser negativo"));
    }

    [Fact]
    public void Validate_WhenSaldoReservadoNegativo_HasError()
    {
        // Arrange / Act
        var result = _sut.Validate(new CreateItemEstoqueRequest
        {
            Codigo = "A",
            Nome = "N",
            Tipo = ItemTipo.Peca,
            UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 1,
            Saldo = 1,
            SaldoReservado = -1
        });

        // Assert
        Assert.Contains(result.Errors, e => e.Contains("Saldo reservado não pode ser negativo"));
    }
}
