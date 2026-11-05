using Application.Services;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

public class OrdemServicoFinalizadaConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<OrdemServicoFinalizadaData>
{
    public string EventType => EventTypes.OrdemServicoFinalizada;

    public async Task HandleAsync(EventEnvelope<OrdemServicoFinalizadaData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ReservaEstoqueService>();
        await service.ProcessarFinalizacaoAsync(envelope.EventId, envelope.Data, cancellationToken);
    }
}
