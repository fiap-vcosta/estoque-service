using Domain.Entities;

namespace Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;

public class CreateItemEstoqueRequest
{
    public string Codigo { get; init; } = string.Empty;
    public required ItemTipo Tipo { get; init; }
    public string Nome { get; init; } = string.Empty;
    public required UnidadeMedida UnidadeMedida { get; init; }
    public required decimal PrecoVenda { get; init; }
    public required decimal Saldo { get; init; }
    public required decimal SaldoReservado { get; init; }
}
