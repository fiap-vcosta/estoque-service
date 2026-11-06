using Domain.Entities;

namespace Application.Abstractions.Gateways;

public interface IItemEstoqueGateway
{
    Task<IReadOnlyList<ItemEstoqueAggregateRoot>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ItemEstoqueAggregateRoot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ItemEstoqueAggregateRoot?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ItemEstoqueAggregateRoot>> GetEBloquearItensAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken = default);
    Task CreateAsync(ItemEstoqueAggregateRoot item, CancellationToken cancellationToken = default);
    Task UpdateAsync(ItemEstoqueAggregateRoot item, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
