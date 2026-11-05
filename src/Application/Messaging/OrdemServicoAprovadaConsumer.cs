using Application.Services;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

public class OrdemServicoAprovadaConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<OrdemServicoAprovadaData>
{
    public string EventType => EventTypes.OrdemServicoAprovada;

    public async Task HandleAsync(EventEnvelope<OrdemServicoAprovadaData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ReservaEstoqueService>();
        await service.ProcessarAprovacaoAsync(envelope.EventId, envelope.Data, cancellationToken);
    }
}
