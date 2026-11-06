using Application.Abstractions.Gateways;
using Application.UseCases.ItemEstoque.Responses;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.UseCases.ItemEstoque.Commands.CreateItemEstoque;

public class CreateItemEstoqueCommandHandler(IItemEstoqueGateway itemEstoqueGateway)
    : IRequestHandler<CreateItemEstoqueCommand, ItemEstoqueResponse>
{
    public async Task<ItemEstoqueResponse> Handle(CreateItemEstoqueCommand request, CancellationToken cancellationToken)
    {
        var existing = await itemEstoqueGateway.GetByCodigoAsync(request.Codigo, cancellationToken);
        if (existing is not null)
        {
            throw new BusinessRuleException("Já existe um item de estoque com este código.");
        }

        if (request.Saldo < 0 || request.SaldoReservado < 0)
        {
            throw new BusinessRuleException("Saldo e saldo reservado não podem ser negativos.");
        }

        if (request.SaldoReservado > request.Saldo)
        {
            throw new BusinessRuleException("Saldo reservado não pode ser maior que o saldo.");
        }

        if (request.PrecoVenda <= 0)
        {
            throw new BusinessRuleException("Preço de venda deve ser maior que zero.");
        }

        var item = new ItemEstoqueAggregateRoot
        {
            Codigo = request.Codigo,
            Tipo = request.Tipo,
            Nome = request.Nome,
            UnidadeMedida = request.UnidadeMedida,
            PrecoVenda = request.PrecoVenda,
            Saldo = request.Saldo,
            SaldoReservado = request.SaldoReservado
        };

        await itemEstoqueGateway.CreateAsync(item, cancellationToken);
        return ItemEstoqueResponse.FromAggregateRoot(item);
    }
}
