using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrenteCaixa.Caixa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaixaAberturaFechamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "caixas_operacionais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperadorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValorInicial = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorFechamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AbertoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caixas_operacionais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "movimentacoes_caixa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaixaId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperadorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    CriadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimentacoes_caixa", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_caixas_operacionais_OperadorId",
                table: "caixas_operacionais",
                column: "OperadorId");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacoes_caixa_CaixaId",
                table: "movimentacoes_caixa",
                column: "CaixaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "caixas_operacionais");

            migrationBuilder.DropTable(
                name: "movimentacoes_caixa");
        }
    }
}
