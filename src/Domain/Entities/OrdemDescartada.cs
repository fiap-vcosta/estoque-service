namespace Domain.Entities;

public class OrdemDescartada
{
    public Guid OrdemServicoId { get; set; }
    public DateTimeOffset DescartadaEm { get; set; }
}
