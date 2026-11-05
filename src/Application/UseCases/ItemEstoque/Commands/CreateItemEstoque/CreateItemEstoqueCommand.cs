using Application.UseCases.ItemEstoque.Responses;
using Domain.Entities;
using MediatR;

namespace Application.UseCases.ItemEstoque.Commands.CreateItemEstoque;

public class CreateItemEstoqueCommand : IRequest<ItemEstoqueResponse>
{
    public string Codigo { get; init; } = string.Empty;
    public ItemTipo Tipo { get; init; }
    public string Nome { get; init; } = string.Empty;
    public UnidadeMedida UnidadeMedida { get; init; }
    public decimal PrecoVenda { get; init; }
    public decimal Saldo { get; init; }
    public decimal SaldoReservado { get; init; }
}
