using Domain.Entities;

namespace Application.UseCases.ItemEstoque.Responses;

public class ItemEstoqueResponse
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public ItemTipo Tipo { get; init; }
    public string Nome { get; init; } = string.Empty;
    public UnidadeMedida UnidadeMedida { get; init; }
    public decimal PrecoVenda { get; init; }
    public decimal Saldo { get; init; }
    public decimal SaldoReservado { get; init; }

    public static ItemEstoqueResponse FromAggregateRoot(ItemEstoqueAggregateRoot item) => new()
    {
        Id = item.Id,
        Codigo = item.Codigo,
        Tipo = item.Tipo,
        Nome = item.Nome,
        UnidadeMedida = item.UnidadeMedida,
        PrecoVenda = item.PrecoVenda,
        Saldo = item.Saldo,
        SaldoReservado = item.SaldoReservado
    };
}
