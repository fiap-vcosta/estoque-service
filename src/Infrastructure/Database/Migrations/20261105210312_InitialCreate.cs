using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventosProcessados",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    ChaveNegocio = table.Column<string>(type: "text", nullable: false),
                    ProcessadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosProcessados", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "ItensEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    UnidadeMedida = table.Column<string>(type: "text", nullable: false),
                    PrecoVenda = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    SaldoReservado = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensEstoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrdensDescartadas",
                columns: table => new
                {
                    OrdemServicoId = table.Column<int>(type: "integer", nullable: false),
                    DescartadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensDescartadas", x => x.OrdemServicoId);
                });

            migrationBuilder.CreateTable(
                name: "ReservasEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdemServicoId = table.Column<int>(type: "integer", nullable: false),
                    AprovadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservasEstoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservaEstoqueItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservaEstoqueId = table.Column<int>(type: "integer", nullable: false),
                    ItemEstoqueId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservaEstoqueItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservaEstoqueItens_ReservasEstoque_ReservaEstoqueId",
                        column: x => x.ReservaEstoqueId,
                        principalTable: "ReservasEstoque",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosProcessados_Tipo_ChaveNegocio",
                table: "EventosProcessados",
                columns: new[] { "Tipo", "ChaveNegocio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensEstoque_Codigo",
                table: "ItensEstoque",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservaEstoqueItens_ReservaEstoqueId",
                table: "ReservaEstoqueItens",
                column: "ReservaEstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasEstoque_OrdemServicoId_AprovadaEm",
                table: "ReservasEstoque",
                columns: new[] { "OrdemServicoId", "AprovadaEm" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosProcessados");

            migrationBuilder.DropTable(
                name: "ItensEstoque");

            migrationBuilder.DropTable(
                name: "OrdensDescartadas");

            migrationBuilder.DropTable(
                name: "ReservaEstoqueItens");

            migrationBuilder.DropTable(
                name: "ReservasEstoque");
        }
    }
}
