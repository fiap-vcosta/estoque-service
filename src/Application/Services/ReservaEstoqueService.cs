using Application.Abstractions.Gateways;
using Application.Abstractions.Persistence;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Domain.Entities;

namespace Application.Services;

public class ReservaEstoqueService(
    IItemEstoqueGateway itemEstoqueGateway,
    IReservaEstoqueGateway reservaEstoqueGateway,
    IEventoProcessadoGateway eventoProcessadoGateway,
    IOrdemDescartadaGateway ordemDescartadaGateway,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
{
    public async Task ProcessarAprovacaoAsync(
        Guid eventId,
        OrdemServicoAprovadaData data,
        CancellationToken cancellationToken = default)
    {
        var chave = ChaveAprovacao(data.OrdemServicoId, data.AprovadaEm);

        if (await eventoProcessadoGateway.ExisteEventIdAsync(eventId, cancellationToken)
            || await eventoProcessadoGateway.ExisteChaveNegocioAsync(EventTypes.OrdemServicoAprovada, chave, cancellationToken))
        {
            return;
        }

        if (await ordemDescartadaGateway.ExisteAsync(data.OrdemServicoId, cancellationToken))
        {
            await MarcarProcessadoAsync(eventId, EventTypes.OrdemServicoAprovada, chave, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        if (await reservaEstoqueGateway.GetByOrdemEAprovacaoAsync(data.OrdemServicoId, data.AprovadaEm, cancellationToken) is not null)
        {
            await MarcarProcessadoAsync(eventId, EventTypes.OrdemServicoAprovada, chave, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        EstoqueReservadoData? reservado = null;
        EstoqueEmFaltaData? emFalta = null;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (await eventoProcessadoGateway.ExisteEventIdAsync(eventId, ct)
                || await eventoProcessadoGateway.ExisteChaveNegocioAsync(EventTypes.OrdemServicoAprovada, chave, ct))
            {
                return;
            }

            if (await ordemDescartadaGateway.ExisteAsync(data.OrdemServicoId, ct))
            {
                await MarcarProcessadoAsync(eventId, EventTypes.OrdemServicoAprovada, chave, ct);
                return;
            }

            var resultado = await TentarReservarInternoAsync(data.OrdemServicoId, data.AprovadaEm, data.Itens, ct);
            reservado = resultado.Reservado;
            emFalta = resultado.EmFalta;

            await MarcarProcessadoAsync(eventId, EventTypes.OrdemServicoAprovada, chave, ct);
        }, cancellationToken);

        if (reservado is not null)
        {
            await eventPublisher.PublishAsync(
                TopicIds.EstoqueReservado,
                EventTypes.EstoqueReservado,
                reservado,
                cancellationToken);
        }
        else if (emFalta is not null)
        {
            await eventPublisher.PublishAsync(
                TopicIds.EstoqueEmFalta,
                EventTypes.EstoqueEmFalta,
                emFalta,
                cancellationToken);
        }
    }

    public async Task ReavaliarPendenciasPorItemAsync(int itemEstoqueId, CancellationToken cancellationToken = default)
    {
        var pendentes = await reservaEstoqueGateway.GetPendentesPorItemOrdenadasAsync(itemEstoqueId, cancellationToken);
        foreach (var pendente in pendentes)
        {
            EstoqueReservadoData? reservado = null;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var atual = await reservaEstoqueGateway.GetByOrdemEAprovacaoAsync(
                    pendente.OrdemServicoId,
                    pendente.AprovadaEm,
                    ct);
                if (atual is null || atual.Status != ReservaEstoqueStatus.Pendente)
                {
                    return;
                }

                var itensPedido = atual.Itens
                    .Select(i => new OrdemServicoAprovadaItem
                    {
                        ItemEstoqueId = i.ItemEstoqueId,
                        Quantidade = i.Quantidade
                    })
                    .ToList();

                var ids = itensPedido.Select(i => i.ItemEstoqueId).Distinct().OrderBy(id => id).ToList();
                var bloqueados = (await itemEstoqueGateway.GetEBloquearItensAsync(ids, ct))
                    .ToDictionary(i => i.Id);

                if (!TodosCabem(itensPedido, bloqueados))
                {
                    return;
                }

                foreach (var grupo in AgruparQuantidades(itensPedido))
                {
                    bloqueados[grupo.Key].TravarEstoque(grupo.Value);
                    await itemEstoqueGateway.UpdateAsync(bloqueados[grupo.Key], ct);
                }

                atual.MarcarReservada();
                await reservaEstoqueGateway.UpdateAsync(atual, ct);

                reservado = new EstoqueReservadoData
                {
                    OrdemServicoId = atual.OrdemServicoId,
                    AprovadaEm = atual.AprovadaEm,
                    Itens = atual.Itens.Select(i => new EstoqueReservadoItem
                    {
                        ItemEstoqueId = i.ItemEstoqueId,
                        Quantidade = i.Quantidade,
                        Status = ItemEstoqueEventoStatus.EstoqueTravado
                    }).ToList()
                };
            }, cancellationToken);

            if (reservado is not null)
            {
                await eventPublisher.PublishAsync(
                    TopicIds.EstoqueReservado,
                    EventTypes.EstoqueReservado,
                    reservado,
                    cancellationToken);
            }
        }
    }

    public async Task ProcessarFinalizacaoAsync(
        Guid eventId,
        OrdemServicoFinalizadaData data,
        CancellationToken cancellationToken = default)
    {
        var chave = data.OrdemServicoId.ToString();
        if (await eventoProcessadoGateway.ExisteEventIdAsync(eventId, cancellationToken)
            || await eventoProcessadoGateway.ExisteChaveNegocioAsync(EventTypes.OrdemServicoFinalizada, chave, cancellationToken))
        {
            return;
        }

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (await eventoProcessadoGateway.ExisteEventIdAsync(eventId, ct)
                || await eventoProcessadoGateway.ExisteChaveNegocioAsync(EventTypes.OrdemServicoFinalizada, chave, ct))
            {
                return;
            }

            var reserva = await reservaEstoqueGateway.GetReservadaByOrdemServicoIdAsync(data.OrdemServicoId, ct);
            if (reserva is not null)
            {
                var ids = reserva.Itens.Select(i => i.ItemEstoqueId).Distinct().OrderBy(id => id).ToList();
                var bloqueados = (await itemEstoqueGateway.GetEBloquearItensAsync(ids, ct))
                    .ToDictionary(i => i.Id);

                foreach (var grupo in reserva.Itens.GroupBy(i => i.ItemEstoqueId))
                {
                    var quantidade = grupo.Sum(i => i.Quantidade);
                    bloqueados[grupo.Key].ConfirmarUtilizacao(quantidade);
                    await itemEstoqueGateway.UpdateAsync(bloqueados[grupo.Key], ct);
                }

                reserva.MarcarConsumida();
                await reservaEstoqueGateway.UpdateAsync(reserva, ct);
            }

            await MarcarProcessadoAsync(eventId, EventTypes.OrdemServicoFinalizada, chave, ct);
        }, cancellationToken);
    }

    public async Task ProcessarDescarteAsync(
        Guid eventId,
        OrdemServicoDescartadaData data,
        CancellationToken cancellationToken = default)
    {
        var chave = data.OrdemServicoId.ToString();
        if (await eventoProcessadoGateway.ExisteEventIdAsync(eventId, cancellationToken)
            || await eventoProcessadoGateway.ExisteChaveNegocioAsync(EventTypes.OrdemServicoDescartada, chave, cancellationToken))
        {
            return;
        }

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (await eventoProcessadoGateway.ExisteEventIdAsync(eventId, ct)
                || await eventoProcessadoGateway.ExisteChaveNegocioAsync(EventTypes.OrdemServicoDescartada, chave, ct))
            {
                return;
            }

            if (!await ordemDescartadaGateway.ExisteAsync(data.OrdemServicoId, ct))
            {
                await ordemDescartadaGateway.AddAsync(new OrdemDescartada
                {
                    OrdemServicoId = data.OrdemServicoId,
                    DescartadaEm = DateTimeOffset.UtcNow
                }, ct);
            }

            var reserva = await reservaEstoqueGateway.GetAtivaByOrdemServicoIdAsync(data.OrdemServicoId, ct);
            if (reserva is not null)
            {
                if (reserva.Status == ReservaEstoqueStatus.Reservada)
                {
                    var ids = reserva.Itens.Select(i => i.ItemEstoqueId).Distinct().OrderBy(id => id).ToList();
                    var bloqueados = (await itemEstoqueGateway.GetEBloquearItensAsync(ids, ct))
                        .ToDictionary(i => i.Id);

                    foreach (var grupo in reserva.Itens.GroupBy(i => i.ItemEstoqueId))
                    {
                        var quantidade = grupo.Sum(i => i.Quantidade);
                        bloqueados[grupo.Key].LiberarReserva(quantidade);
                        await itemEstoqueGateway.UpdateAsync(bloqueados[grupo.Key], ct);
                    }
                }

                reserva.MarcarLiberada();
                await reservaEstoqueGateway.UpdateAsync(reserva, ct);
            }

            await MarcarProcessadoAsync(eventId, EventTypes.OrdemServicoDescartada, chave, ct);
        }, cancellationToken);
    }

    private async Task<(EstoqueReservadoData? Reservado, EstoqueEmFaltaData? EmFalta)> TentarReservarInternoAsync(
        int ordemServicoId,
        DateTimeOffset aprovadaEm,
        IReadOnlyList<OrdemServicoAprovadaItem> itens,
        CancellationToken cancellationToken)
    {
        var ids = itens.Select(i => i.ItemEstoqueId).Distinct().OrderBy(id => id).ToList();
        var bloqueados = (await itemEstoqueGateway.GetEBloquearItensAsync(ids, cancellationToken))
            .ToDictionary(i => i.Id);

        if (ids.Any(id => !bloqueados.ContainsKey(id)))
        {
            var faltantes = ids.Where(id => !bloqueados.ContainsKey(id)).ToHashSet();
            var reserva = ReservaEstoque.CriarPendente(
                ordemServicoId,
                aprovadaEm,
                itens.Select(i => new ReservaEstoqueItem
                {
                    ItemEstoqueId = i.ItemEstoqueId,
                    Quantidade = i.Quantidade
                }));
            await reservaEstoqueGateway.AddAsync(reserva, cancellationToken);

            return (null, new EstoqueEmFaltaData
            {
                OrdemServicoId = ordemServicoId,
                AprovadaEm = aprovadaEm,
                Itens = itens.Select(i => new EstoqueEmFaltaItem
                {
                    ItemEstoqueId = i.ItemEstoqueId,
                    Quantidade = i.Quantidade,
                    Status = faltantes.Contains(i.ItemEstoqueId)
                        ? ItemEstoqueEventoStatus.EstoqueEmFalta
                        : ItemEstoqueEventoStatus.EstoqueDisponivel
                }).ToList()
            });
        }

        if (!TodosCabem(itens, bloqueados))
        {
            var disponivelPorItem = new Dictionary<int, decimal>();
            foreach (var grupo in AgruparQuantidades(itens))
            {
                disponivelPorItem[grupo.Key] = bloqueados[grupo.Key].SaldoDisponivel;
            }

            var restante = new Dictionary<int, decimal>(disponivelPorItem);
            var statusLinhas = new List<EstoqueEmFaltaItem>();
            foreach (var linha in itens)
            {
                var cabe = restante[linha.ItemEstoqueId] >= linha.Quantidade;
                if (cabe)
                {
                    restante[linha.ItemEstoqueId] -= linha.Quantidade;
                }

                statusLinhas.Add(new EstoqueEmFaltaItem
                {
                    ItemEstoqueId = linha.ItemEstoqueId,
                    Quantidade = linha.Quantidade,
                    Status = cabe
                        ? ItemEstoqueEventoStatus.EstoqueDisponivel
                        : ItemEstoqueEventoStatus.EstoqueEmFalta
                });
            }

            var pendente = ReservaEstoque.CriarPendente(
                ordemServicoId,
                aprovadaEm,
                itens.Select(i => new ReservaEstoqueItem
                {
                    ItemEstoqueId = i.ItemEstoqueId,
                    Quantidade = i.Quantidade
                }));
            await reservaEstoqueGateway.AddAsync(pendente, cancellationToken);

            return (null, new EstoqueEmFaltaData
            {
                OrdemServicoId = ordemServicoId,
                AprovadaEm = aprovadaEm,
                Itens = statusLinhas
            });
        }

        foreach (var grupo in AgruparQuantidades(itens))
        {
            bloqueados[grupo.Key].TravarEstoque(grupo.Value);
            await itemEstoqueGateway.UpdateAsync(bloqueados[grupo.Key], cancellationToken);
        }

        var reservada = ReservaEstoque.CriarReservada(
            ordemServicoId,
            aprovadaEm,
            itens.Select(i => new ReservaEstoqueItem
            {
                ItemEstoqueId = i.ItemEstoqueId,
                Quantidade = i.Quantidade
            }));
        await reservaEstoqueGateway.AddAsync(reservada, cancellationToken);

        return (new EstoqueReservadoData
        {
            OrdemServicoId = ordemServicoId,
            AprovadaEm = aprovadaEm,
            Itens = itens.Select(i => new EstoqueReservadoItem
            {
                ItemEstoqueId = i.ItemEstoqueId,
                Quantidade = i.Quantidade,
                Status = ItemEstoqueEventoStatus.EstoqueTravado
            }).ToList()
        }, null);
    }

    private static bool TodosCabem(
        IReadOnlyList<OrdemServicoAprovadaItem> itens,
        IReadOnlyDictionary<int, ItemEstoqueAggregateRoot> bloqueados)
    {
        foreach (var grupo in AgruparQuantidades(itens))
        {
            if (!bloqueados.TryGetValue(grupo.Key, out var item))
            {
                return false;
            }

            if (item.SaldoDisponivel < grupo.Value)
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<KeyValuePair<int, decimal>> AgruparQuantidades(
        IReadOnlyList<OrdemServicoAprovadaItem> itens) =>
        itens.GroupBy(i => i.ItemEstoqueId)
            .Select(g => new KeyValuePair<int, decimal>(g.Key, g.Sum(i => i.Quantidade)));

    private async Task MarcarProcessadoAsync(
        Guid eventId,
        string tipo,
        string chaveNegocio,
        CancellationToken cancellationToken)
    {
        await eventoProcessadoGateway.AddAsync(new EventoProcessado
        {
            EventId = eventId,
            Tipo = tipo,
            ChaveNegocio = chaveNegocio,
            ProcessadoEm = DateTimeOffset.UtcNow
        }, cancellationToken);
    }

    public static string ChaveAprovacao(int ordemServicoId, DateTimeOffset aprovadaEm) =>
        $"{ordemServicoId}|{aprovadaEm.UtcDateTime:O}";
}
