using Application.UseCases.ItemEstoque.Responses;
using MediatR;

namespace Application.UseCases.ItemEstoque.Queries.GetItemEstoqueById;

public class GetItemEstoqueByIdQuery : IRequest<ItemEstoqueResponse?>
{
    public int Id { get; init; }
}
