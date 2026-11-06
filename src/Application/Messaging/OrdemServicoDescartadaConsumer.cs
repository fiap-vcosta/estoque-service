using System.Diagnostics.CodeAnalysis;
using Application.Services;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging;

[ExcludeFromCodeCoverage(Justification = "Thin DI adapter; covered via ReservaEstoqueService tests.")]
public class OrdemServicoDescartadaConsumer(IServiceScopeFactory scopeFactory)
    : IIntegrationEventConsumer<OrdemServicoDescartadaData>
{
    public string EventType => EventTypes.OrdemServicoDescartada;

    public async Task HandleAsync(EventEnvelope<OrdemServicoDescartadaData> envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ReservaEstoqueService>();
        await service.ProcessarDescarteAsync(envelope.EventId, envelope.Data, cancellationToken);
    }
}
