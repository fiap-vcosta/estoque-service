using Domain.Entities;

namespace Application.Abstractions.Gateways;

public interface IReservaEstoqueGateway
{
    Task<ReservaEstoque?> GetByOrdemEAprovacaoAsync(int ordemServicoId, DateTimeOffset aprovadaEm, CancellationToken cancellationToken = default);
    Task<ReservaEstoque?> GetReservadaByOrdemServicoIdAsync(int ordemServicoId, CancellationToken cancellationToken = default);
    Task<ReservaEstoque?> GetAtivaByOrdemServicoIdAsync(int ordemServicoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReservaEstoque>> GetPendentesPorItemOrdenadasAsync(int itemEstoqueId, CancellationToken cancellationToken = default);
    Task AddAsync(ReservaEstoque reserva, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReservaEstoque reserva, CancellationToken cancellationToken = default);
}
