using Application.Abstractions.Gateways;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Gateways;

public class ReservaEstoqueGateway(AppDbContext context) : IReservaEstoqueGateway
{
    public async Task<ReservaEstoque?> GetByOrdemEAprovacaoAsync(
        int ordemServicoId,
        DateTimeOffset aprovadaEm,
        CancellationToken cancellationToken = default) =>
        await context.ReservasEstoque
            .Include(r => r.Itens)
            .FirstOrDefaultAsync(
                r => r.OrdemServicoId == ordemServicoId && r.AprovadaEm == aprovadaEm,
                cancellationToken);

    public async Task<ReservaEstoque?> GetReservadaByOrdemServicoIdAsync(
        int ordemServicoId,
        CancellationToken cancellationToken = default) =>
        await context.ReservasEstoque
            .Include(r => r.Itens)
            .FirstOrDefaultAsync(
                r => r.OrdemServicoId == ordemServicoId && r.Status == ReservaEstoqueStatus.Reservada,
                cancellationToken);

    public async Task<ReservaEstoque?> GetAtivaByOrdemServicoIdAsync(
        int ordemServicoId,
        CancellationToken cancellationToken = default) =>
        await context.ReservasEstoque
            .Include(r => r.Itens)
            .Where(r => r.OrdemServicoId == ordemServicoId
                        && (r.Status == ReservaEstoqueStatus.Pendente
                            || r.Status == ReservaEstoqueStatus.Reservada))
            .OrderByDescending(r => r.AprovadaEm)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ReservaEstoque>> GetPendentesPorItemOrdenadasAsync(
        int itemEstoqueId,
        CancellationToken cancellationToken = default) =>
        await context.ReservasEstoque
            .Include(r => r.Itens)
            .Where(r => r.Status == ReservaEstoqueStatus.Pendente
                        && r.Itens.Any(i => i.ItemEstoqueId == itemEstoqueId))
            .OrderBy(r => r.AprovadaEm)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ReservaEstoque reserva, CancellationToken cancellationToken = default)
    {
        context.ReservasEstoque.Add(reserva);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ReservaEstoque reserva, CancellationToken cancellationToken = default)
    {
        context.ReservasEstoque.Update(reserva);
        await context.SaveChangesAsync(cancellationToken);
    }
}
