using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrenteCaixa.Caixa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaixaResumoTurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_mensagens",
                columns: table => new
                {
                    MensagemId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutingKey = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ConsumidoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_mensagens", x => x.MensagemId);
                });

            migrationBuilder.CreateTable(
                name: "vendas_caixa_projetadas",
                columns: table => new
                {
                    VendaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaixaId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperadorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PagamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormaPagamento = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OcorridaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProjetadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendas_caixa_projetadas", x => x.VendaId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vendas_caixa_projetadas_CaixaId",
                table: "vendas_caixa_projetadas",
                column: "CaixaId");

            migrationBuilder.CreateIndex(
                name: "IX_vendas_caixa_projetadas_OperadorId",
                table: "vendas_caixa_projetadas",
                column: "OperadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_mensagens");

            migrationBuilder.DropTable(
                name: "vendas_caixa_projetadas");
        }
    }
}
