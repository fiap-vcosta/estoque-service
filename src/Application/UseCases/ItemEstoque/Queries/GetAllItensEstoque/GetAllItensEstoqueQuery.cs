using Application.UseCases.ItemEstoque.Responses;
using MediatR;

namespace Application.UseCases.ItemEstoque.Queries.GetAllItensEstoque;

public class GetAllItensEstoqueQuery : IRequest<IReadOnlyList<ItemEstoqueResponse>>;
