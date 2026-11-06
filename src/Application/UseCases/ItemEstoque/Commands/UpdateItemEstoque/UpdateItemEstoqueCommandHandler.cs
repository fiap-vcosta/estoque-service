using Application.Abstractions.Gateways;
using Application.UseCases.ItemEstoque.Responses;
using Domain.Exceptions;
using MediatR;

namespace Application.UseCases.ItemEstoque.Commands.UpdateItemEstoque;

public class UpdateItemEstoqueCommandHandler(IItemEstoqueGateway itemEstoqueGateway)
    : IRequestHandler<UpdateItemEstoqueCommand, ItemEstoqueResponse>
{
    public async Task<ItemEstoqueResponse> Handle(UpdateItemEstoqueCommand request, CancellationToken cancellationToken)
    {
        var item = await itemEstoqueGateway.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainNotFoundException($"Item de estoque com id {request.Id} não encontrado");

        var byCodigo = await itemEstoqueGateway.GetByCodigoAsync(request.Codigo, cancellationToken);
        if (byCodigo is not null && byCodigo.Id != request.Id)
        {
            throw new BusinessRuleException("Já existe um item de estoque com este código.");
        }

        if (request.PrecoVenda <= 0)
        {
            throw new BusinessRuleException("Preço de venda deve ser maior que zero.");
        }

        item.Codigo = request.Codigo;
        item.Tipo = request.Tipo;
        item.Nome = request.Nome;
        item.UnidadeMedida = request.UnidadeMedida;
        item.PrecoVenda = request.PrecoVenda;

        await itemEstoqueGateway.UpdateAsync(item, cancellationToken);
        return ItemEstoqueResponse.FromAggregateRoot(item);
    }
}
