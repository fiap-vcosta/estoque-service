using Api.ViewModels.ItemEstoque;
using Application.UseCases.ItemEstoque.Responses;

namespace Api.Presenters.ItemEstoque;

public class ItemEstoquePresenter
{
    public ItemEstoqueViewModel Present(ItemEstoqueResponse response) => new()
    {
        Id = response.Id,
        Codigo = response.Codigo,
        Tipo = response.Tipo,
        Nome = response.Nome,
        UnidadeMedida = response.UnidadeMedida,
        PrecoVenda = response.PrecoVenda,
        Saldo = response.Saldo,
        SaldoReservado = response.SaldoReservado
    };

    public IEnumerable<ItemEstoqueViewModel> Present(IEnumerable<ItemEstoqueResponse> responses) =>
        responses.Select(Present);
}
