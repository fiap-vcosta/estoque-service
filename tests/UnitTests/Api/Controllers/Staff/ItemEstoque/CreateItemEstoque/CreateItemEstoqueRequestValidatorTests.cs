using Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;

namespace UnitTests.Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;

public class CreateItemEstoqueRequestValidatorTests
{
    private readonly CreateItemEstoqueRequestValidator _sut = new();

    [Fact]
    public void Validate_WhenEmpty_HasErrors()
    {
        // Arrange / Act
        var result = _sut.Validate(new CreateItemEstoqueRequest());

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
            Codigo = "A", Nome = "N", PrecoVenda = 1, Saldo = 2, SaldoReservado = 1
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
            Codigo = "A", Nome = "N", PrecoVenda = 1, Saldo = 1, SaldoReservado = 2
        });

        // Assert
        Assert.Contains(result.Errors, e => e.Contains("reservado"));
    }
}
