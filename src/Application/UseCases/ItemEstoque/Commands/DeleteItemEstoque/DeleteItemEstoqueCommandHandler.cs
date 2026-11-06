using Application.Abstractions.Gateways;
using Domain.Exceptions;
using MediatR;

namespace Application.UseCases.ItemEstoque.Commands.DeleteItemEstoque;

public class DeleteItemEstoqueCommandHandler(IItemEstoqueGateway itemEstoqueGateway)
    : IRequestHandler<DeleteItemEstoqueCommand>
{
    public async Task Handle(DeleteItemEstoqueCommand request, CancellationToken cancellationToken)
    {
        var item = await itemEstoqueGateway.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainNotFoundException($"Item de estoque com id {request.Id} não encontrado");

        await itemEstoqueGateway.DeleteAsync(item.Id, cancellationToken);
    }
}
