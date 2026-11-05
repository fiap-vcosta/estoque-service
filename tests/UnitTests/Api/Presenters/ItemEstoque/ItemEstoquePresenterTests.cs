using Api.Presenters.ItemEstoque;
using Application.UseCases.ItemEstoque.Responses;
using Domain.Entities;

namespace UnitTests.Api.Presenters.ItemEstoque;

public class ItemEstoquePresenterTests
{
    [Fact]
    public void Present_MapsFields()
    {
        // Arrange
        var presenter = new ItemEstoquePresenter();
        var response = new ItemEstoqueResponse
        {
            Id = 1, Codigo = "C", Tipo = ItemTipo.Insumo, Nome = "N",
            UnidadeMedida = UnidadeMedida.Litro, PrecoVenda = 2, Saldo = 3, SaldoReservado = 1
        };

        // Act
        var vm = presenter.Present(response);
        var list = presenter.Present([response]).ToList();

        // Assert
        Assert.Equal(1, vm.Id);
        Assert.Single(list);
    }
}
