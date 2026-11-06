using Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;
using Domain.Entities;

namespace UnitTests.Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;

public class UpdateItemEstoqueRequestValidatorTests
{
    [Fact]
    public void Validate_WhenEmpty_HasErrors()
    {
        // Arrange / Act
        var result = new UpdateItemEstoqueRequestValidator().Validate(new UpdateItemEstoqueRequest
        {
            Tipo = default,
            UnidadeMedida = default,
            PrecoVenda = default
        });

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenOk_IsValid()
    {
        // Arrange / Act
        var result = new UpdateItemEstoqueRequestValidator().Validate(new UpdateItemEstoqueRequest
        {
            Codigo = "A",
            Nome = "N",
            Tipo = ItemTipo.Peca,
            UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 1
        });

        // Assert
        Assert.True(result.IsValid);
    }
}
