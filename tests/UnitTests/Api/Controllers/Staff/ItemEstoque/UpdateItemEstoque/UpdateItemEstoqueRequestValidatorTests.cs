using Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;

namespace UnitTests.Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;

public class UpdateItemEstoqueRequestValidatorTests
{
    [Fact]
    public void Validate_WhenEmpty_HasErrors()
    {
        // Arrange / Act
        var result = new UpdateItemEstoqueRequestValidator().Validate(new UpdateItemEstoqueRequest());

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenOk_IsValid()
    {
        // Arrange / Act
        var result = new UpdateItemEstoqueRequestValidator().Validate(new UpdateItemEstoqueRequest
        {
            Codigo = "A", Nome = "N", PrecoVenda = 1
        });

        // Assert
        Assert.True(result.IsValid);
    }
}
