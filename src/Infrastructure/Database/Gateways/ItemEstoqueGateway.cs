using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Gateways;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Gateways;

public class ItemEstoqueGateway(AppDbContext context) : IItemEstoqueGateway
{
    public async Task<IReadOnlyList<ItemEstoqueAggregateRoot>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.ItensEstoque.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<ItemEstoqueAggregateRoot?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await context.ItensEstoque.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<ItemEstoqueAggregateRoot?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
        await context.ItensEstoque.FirstOrDefaultAsync(i => i.Codigo == codigo, cancellationToken);

    [ExcludeFromCodeCoverage(Justification = "Postgres FOR UPDATE; covered by IntegrationTests.")]
    public async Task<IReadOnlyList<ItemEstoqueAggregateRoot>> GetEBloquearItensAsync(
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var idsOrdenados = ids.Distinct().OrderBy(id => id).ToArray();
        return await context.ItensEstoque
            .FromSql($"SELECT * FROM \"ItensEstoque\" WHERE \"Id\" = ANY({idsOrdenados}) FOR UPDATE")
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(ItemEstoqueAggregateRoot item, CancellationToken cancellationToken = default)
    {
        context.ItensEstoque.Add(item);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ItemEstoqueAggregateRoot item, CancellationToken cancellationToken = default)
    {
        context.ItensEstoque.Update(item);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await context.ItensEstoque.FindAsync([id], cancellationToken);
        if (item is null)
        {
            return;
        }

        context.ItensEstoque.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
    }
}
