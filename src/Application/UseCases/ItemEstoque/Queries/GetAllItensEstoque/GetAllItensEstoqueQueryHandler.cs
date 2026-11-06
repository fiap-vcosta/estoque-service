using Application.Abstractions.Gateways;
using Application.UseCases.ItemEstoque.Responses;
using MediatR;

namespace Application.UseCases.ItemEstoque.Queries.GetAllItensEstoque;

public class GetAllItensEstoqueQueryHandler(IItemEstoqueGateway itemEstoqueGateway)
    : IRequestHandler<GetAllItensEstoqueQuery, IReadOnlyList<ItemEstoqueResponse>>
{
    public async Task<IReadOnlyList<ItemEstoqueResponse>> Handle(GetAllItensEstoqueQuery request, CancellationToken cancellationToken)
    {
        var itens = await itemEstoqueGateway.GetAllAsync(cancellationToken);
        return itens.Select(ItemEstoqueResponse.FromAggregateRoot).ToList();
    }
}
