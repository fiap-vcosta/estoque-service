using Application.Abstractions.Gateways;
using Application.UseCases.ItemEstoque.Responses;
using MediatR;

namespace Application.UseCases.ItemEstoque.Queries.GetItemEstoqueById;

public class GetItemEstoqueByIdQueryHandler(IItemEstoqueGateway itemEstoqueGateway)
    : IRequestHandler<GetItemEstoqueByIdQuery, ItemEstoqueResponse?>
{
    public async Task<ItemEstoqueResponse?> Handle(GetItemEstoqueByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await itemEstoqueGateway.GetByIdAsync(request.Id, cancellationToken);
        return item is null ? null : ItemEstoqueResponse.FromAggregateRoot(item);
    }
}
