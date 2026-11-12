using Domain.Entities;

namespace Application.Abstractions.Gateways;

public interface IOrdemDescartadaGateway
{
    Task<bool> ExisteAsync(Guid ordemServicoId, CancellationToken cancellationToken = default);
    Task AddAsync(OrdemDescartada ordem, CancellationToken cancellationToken = default);
}
