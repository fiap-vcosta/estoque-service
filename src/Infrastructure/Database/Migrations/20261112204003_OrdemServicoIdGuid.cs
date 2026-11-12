using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class OrdemServicoIdGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"ReservaEstoqueItens\";");
            migrationBuilder.Sql("DELETE FROM \"ReservasEstoque\";");
            migrationBuilder.Sql("DELETE FROM \"OrdensDescartadas\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"ReservasEstoque\" ALTER COLUMN \"OrdemServicoId\" TYPE uuid USING gen_random_uuid();");

            migrationBuilder.Sql(
                "ALTER TABLE \"OrdensDescartadas\" ALTER COLUMN \"OrdemServicoId\" TYPE uuid USING gen_random_uuid();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"ReservaEstoqueItens\";");
            migrationBuilder.Sql("DELETE FROM \"ReservasEstoque\";");
            migrationBuilder.Sql("DELETE FROM \"OrdensDescartadas\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"ReservasEstoque\" ALTER COLUMN \"OrdemServicoId\" TYPE integer USING 0;");

            migrationBuilder.Sql(
                "ALTER TABLE \"OrdensDescartadas\" ALTER COLUMN \"OrdemServicoId\" TYPE integer USING 0;");
        }
    }
}
