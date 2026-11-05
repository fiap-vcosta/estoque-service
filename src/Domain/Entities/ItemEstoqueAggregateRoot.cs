namespace Domain.Entities;

public enum ItemTipo
{
    Peca,
    Insumo
}

public enum UnidadeMedida
{
    Unidade,
    Jogo,
    Par,
    Litro,
    Kg,
    mL,
    Frasco
}

public class ItemEstoqueAggregateRoot
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public ItemTipo Tipo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public UnidadeMedida UnidadeMedida { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal Saldo { get; set; }
    public decimal SaldoReservado { get; set; }
    public decimal SaldoDisponivel => Saldo - SaldoReservado;

    public void RegistrarEntradaEstoque(decimal quantidade)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        }

        Saldo += quantidade;
    }

    public void TravarEstoque(decimal quantidade)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        }

        if (quantidade > SaldoDisponivel)
        {
            throw new InvalidOperationException("Saldo disponível insuficiente para reserva.");
        }

        SaldoReservado += quantidade;
    }

    public void LiberarReserva(decimal quantidade)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        }

        if (quantidade > SaldoReservado)
        {
            throw new InvalidOperationException("Quantidade a liberar excede o saldo reservado.");
        }

        SaldoReservado -= quantidade;
    }

    public void ConfirmarUtilizacao(decimal quantidade)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        }

        if (quantidade > SaldoReservado)
        {
            throw new InvalidOperationException("Quantidade a consumir excede o saldo reservado.");
        }

        if (quantidade > Saldo)
        {
            throw new InvalidOperationException("Quantidade a consumir excede o saldo.");
        }

        SaldoReservado -= quantidade;
        Saldo -= quantidade;
    }
}
