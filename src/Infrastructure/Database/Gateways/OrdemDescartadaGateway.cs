using Application.Abstractions.Gateways;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Gateways;

public class OrdemDescartadaGateway(AppDbContext context) : IOrdemDescartadaGateway
{
    public Task<bool> ExisteAsync(Guid ordemServicoId, CancellationToken cancellationToken = default) =>
        context.OrdensDescartadas.AnyAsync(o => o.OrdemServicoId == ordemServicoId, cancellationToken);

    public async Task AddAsync(OrdemDescartada ordem, CancellationToken cancellationToken = default)
    {
        context.OrdensDescartadas.Add(ordem);
        await Task.CompletedTask;
    }
}
