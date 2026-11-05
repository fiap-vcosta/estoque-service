using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ItemEstoqueAggregateRoot> ItensEstoque => Set<ItemEstoqueAggregateRoot>();
    public DbSet<ReservaEstoque> ReservasEstoque => Set<ReservaEstoque>();
    public DbSet<ReservaEstoqueItem> ReservaEstoqueItens => Set<ReservaEstoqueItem>();
    public DbSet<EventoProcessado> EventosProcessados => Set<EventoProcessado>();
    public DbSet<OrdemDescartada> OrdensDescartadas => Set<OrdemDescartada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var itens = modelBuilder.Entity<ItemEstoqueAggregateRoot>();
        itens.ToTable("ItensEstoque");
        itens.HasKey(i => i.Id);
        itens.HasIndex(i => i.Codigo).IsUnique();
        itens.Property(i => i.Codigo).IsRequired();
        itens.Property(i => i.Nome).IsRequired();
        itens.Property(i => i.Tipo).HasConversion<string>().IsRequired();
        itens.Property(i => i.UnidadeMedida).HasConversion<string>().IsRequired();
        itens.Property(i => i.PrecoVenda).HasPrecision(10, 2);
        itens.Property(i => i.Saldo).HasPrecision(18, 4);
        itens.Property(i => i.SaldoReservado).HasPrecision(18, 4);
        itens.Ignore(i => i.SaldoDisponivel);

        var reservas = modelBuilder.Entity<ReservaEstoque>();
        reservas.ToTable("ReservasEstoque");
        reservas.HasKey(r => r.Id);
        reservas.HasIndex(r => new { r.OrdemServicoId, r.AprovadaEm }).IsUnique();
        reservas.Property(r => r.Status).HasConversion<string>().IsRequired();
        reservas.HasMany(r => r.Itens)
            .WithOne()
            .HasForeignKey(i => i.ReservaEstoqueId)
            .OnDelete(DeleteBehavior.Cascade);

        var reservaItens = modelBuilder.Entity<ReservaEstoqueItem>();
        reservaItens.ToTable("ReservaEstoqueItens");
        reservaItens.HasKey(i => i.Id);
        reservaItens.Property(i => i.Quantidade).HasPrecision(18, 4);

        var eventos = modelBuilder.Entity<EventoProcessado>();
        eventos.ToTable("EventosProcessados");
        eventos.HasKey(e => e.EventId);
        eventos.HasIndex(e => new { e.Tipo, e.ChaveNegocio }).IsUnique();
        eventos.Property(e => e.Tipo).IsRequired();
        eventos.Property(e => e.ChaveNegocio).IsRequired();

        var descartadas = modelBuilder.Entity<OrdemDescartada>();
        descartadas.ToTable("OrdensDescartadas");
        descartadas.HasKey(o => o.OrdemServicoId);
        descartadas.Property(o => o.OrdemServicoId).ValueGeneratedNever();
    }
}
