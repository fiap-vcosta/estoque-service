namespace Domain.Entities;

public enum ReservaEstoqueStatus
{
    Pendente,
    Reservada,
    Consumida,
    Liberada
}

public class ReservaEstoque
{
    public int Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public DateTimeOffset AprovadaEm { get; set; }
    public ReservaEstoqueStatus Status { get; set; }
    public List<ReservaEstoqueItem> Itens { get; set; } = [];

    public static ReservaEstoque CriarPendente(Guid ordemServicoId, DateTimeOffset aprovadaEm, IEnumerable<ReservaEstoqueItem> itens)
    {
        return new ReservaEstoque
        {
            OrdemServicoId = ordemServicoId,
            AprovadaEm = aprovadaEm,
            Status = ReservaEstoqueStatus.Pendente,
            Itens = itens.ToList()
        };
    }

    public static ReservaEstoque CriarReservada(Guid ordemServicoId, DateTimeOffset aprovadaEm, IEnumerable<ReservaEstoqueItem> itens)
    {
        return new ReservaEstoque
        {
            OrdemServicoId = ordemServicoId,
            AprovadaEm = aprovadaEm,
            Status = ReservaEstoqueStatus.Reservada,
            Itens = itens.ToList()
        };
    }

    public void MarcarReservada()
    {
        if (Status != ReservaEstoqueStatus.Pendente)
        {
            throw new InvalidOperationException("Somente pendência pode virar reserva.");
        }

        Status = ReservaEstoqueStatus.Reservada;
    }

    public void MarcarConsumida()
    {
        if (Status != ReservaEstoqueStatus.Reservada)
        {
            throw new InvalidOperationException("Somente reserva ativa pode ser consumida.");
        }

        Status = ReservaEstoqueStatus.Consumida;
    }

    public void MarcarLiberada()
    {
        if (Status is not (ReservaEstoqueStatus.Pendente or ReservaEstoqueStatus.Reservada))
        {
            throw new InvalidOperationException("Somente pendência ou reserva ativa pode ser liberada.");
        }

        Status = ReservaEstoqueStatus.Liberada;
    }
}

public class ReservaEstoqueItem
{
    public int Id { get; set; }
    public int ReservaEstoqueId { get; set; }
    public int ItemEstoqueId { get; set; }
    public decimal Quantidade { get; set; }
}
