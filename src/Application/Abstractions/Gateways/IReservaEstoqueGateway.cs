using Domain.Entities;

namespace Application.Abstractions.Gateways;

public interface IReservaEstoqueGateway
{
    Task<ReservaEstoque?> GetByOrdemEAprovacaoAsync(Guid ordemServicoId, DateTimeOffset aprovadaEm, CancellationToken cancellationToken = default);
    Task<ReservaEstoque?> GetReservadaByOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken cancellationToken = default);
    Task<ReservaEstoque?> GetAtivaByOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReservaEstoque>> GetPendentesPorItemOrdenadasAsync(int itemEstoqueId, CancellationToken cancellationToken = default);
    Task AddAsync(ReservaEstoque reserva, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReservaEstoque reserva, CancellationToken cancellationToken = default);
}
