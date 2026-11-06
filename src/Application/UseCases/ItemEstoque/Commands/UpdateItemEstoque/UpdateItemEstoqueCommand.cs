using Application.UseCases.ItemEstoque.Responses;
using Domain.Entities;
using MediatR;

namespace Application.UseCases.ItemEstoque.Commands.UpdateItemEstoque;

public class UpdateItemEstoqueCommand : IRequest<ItemEstoqueResponse>
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public ItemTipo Tipo { get; init; }
    public string Nome { get; init; } = string.Empty;
    public UnidadeMedida UnidadeMedida { get; init; }
    public decimal PrecoVenda { get; init; }
}
