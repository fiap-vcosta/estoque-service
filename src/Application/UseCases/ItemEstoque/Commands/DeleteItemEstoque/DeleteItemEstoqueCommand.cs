using MediatR;

namespace Application.UseCases.ItemEstoque.Commands.DeleteItemEstoque;

public class DeleteItemEstoqueCommand : IRequest
{
    public int Id { get; init; }
}
