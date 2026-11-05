using Application.UseCases.ItemEstoque.Responses;
using MediatR;

namespace Application.UseCases.ItemEstoque.Commands.RegistrarEntradaEstoque;

public class RegistrarEntradaEstoqueCommand : IRequest<ItemEstoqueResponse>
{
    public int IdItemEstoque { get; init; }
    public decimal QuantidadeRecebida { get; init; }
}
