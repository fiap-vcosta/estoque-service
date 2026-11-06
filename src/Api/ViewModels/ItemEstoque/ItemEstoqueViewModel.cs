using Domain.Entities;

namespace Api.ViewModels.ItemEstoque;

public record ItemEstoqueViewModel
{
    public required int Id { get; init; }
    public required string Codigo { get; init; }
    public required ItemTipo Tipo { get; init; }
    public required string Nome { get; init; }
    public required UnidadeMedida UnidadeMedida { get; init; }
    public required decimal PrecoVenda { get; init; }
    public required decimal Saldo { get; init; }
    public required decimal SaldoReservado { get; init; }
}
