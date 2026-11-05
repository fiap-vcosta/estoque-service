using Api.Controllers.Staff.ItemEstoque.RegistrarEntradaEstoque;

namespace UnitTests.Api.Controllers.Staff.ItemEstoque.RegistrarEntradaEstoque;

public class RegistrarEntradaEstoqueRequestValidatorTests
{
    [Fact]
    public void Validate_WhenZero_HasError()
    {
        // Arrange / Act
        var result = new RegistrarEntradaEstoqueRequestValidator().Validate(new RegistrarEntradaEstoqueRequest { Quantidade = 0 });

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPositive_IsValid()
    {
        // Arrange / Act
        var result = new RegistrarEntradaEstoqueRequestValidator().Validate(new RegistrarEntradaEstoqueRequest { Quantidade = 1 });

        // Assert
        Assert.True(result.IsValid);
    }
}
