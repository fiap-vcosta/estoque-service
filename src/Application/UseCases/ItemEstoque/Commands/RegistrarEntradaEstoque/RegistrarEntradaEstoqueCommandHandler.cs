using Application.Abstractions.Gateways;
using Application.Services;
using Application.UseCases.ItemEstoque.Responses;
using Domain.Exceptions;
using MediatR;

namespace Application.UseCases.ItemEstoque.Commands.RegistrarEntradaEstoque;

public class RegistrarEntradaEstoqueCommandHandler(
    IItemEstoqueGateway itemEstoqueGateway,
    ReservaEstoqueService reservaEstoqueService)
    : IRequestHandler<RegistrarEntradaEstoqueCommand, ItemEstoqueResponse>
{
    public async Task<ItemEstoqueResponse> Handle(RegistrarEntradaEstoqueCommand request, CancellationToken cancellationToken)
    {
        if (request.QuantidadeRecebida <= 0)
        {
            throw new BusinessRuleException("Quantidade recebida deve ser maior que zero.");
        }

        var item = await itemEstoqueGateway.GetByIdAsync(request.IdItemEstoque, cancellationToken)
            ?? throw new DomainNotFoundException($"Item de estoque com id {request.IdItemEstoque} não encontrado");

        item.RegistrarEntradaEstoque(request.QuantidadeRecebida);
        await itemEstoqueGateway.UpdateAsync(item, cancellationToken);

        await reservaEstoqueService.ReavaliarPendenciasPorItemAsync(item.Id, cancellationToken);

        var atualizado = await itemEstoqueGateway.GetByIdAsync(item.Id, cancellationToken)
            ?? throw new DomainNotFoundException($"Item de estoque com id {request.IdItemEstoque} não encontrado");

        return ItemEstoqueResponse.FromAggregateRoot(atualizado);
    }
}
