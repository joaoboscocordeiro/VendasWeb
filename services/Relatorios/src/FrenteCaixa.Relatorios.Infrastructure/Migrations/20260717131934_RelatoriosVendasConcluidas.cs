using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrenteCaixa.Relatorios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelatoriosVendasConcluidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_mensagens",
                columns: table => new
                {
                    MensagemId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutingKey = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ConsumidoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_mensagens", x => x.MensagemId);
                });

            migrationBuilder.CreateTable(
                name: "vendas_concluidas",
                columns: table => new
                {
                    VendaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaixaId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperadorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PagamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormaPagamento = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ConcluidaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProjetadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendas_concluidas", x => x.VendaId);
                });

            migrationBuilder.CreateTable(
                name: "itens_vendas_concluidas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DescricaoProduto = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_vendas_concluidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_itens_vendas_concluidas_vendas_concluidas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "vendas_concluidas",
                        principalColumn: "VendaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_mensagens_RoutingKey",
                table: "inbox_mensagens",
                column: "RoutingKey");

            migrationBuilder.CreateIndex(
                name: "IX_itens_vendas_concluidas_ProdutoId",
                table: "itens_vendas_concluidas",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_itens_vendas_concluidas_VendaId",
                table: "itens_vendas_concluidas",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_vendas_concluidas_ConcluidaEm",
                table: "vendas_concluidas",
                column: "ConcluidaEm");

            migrationBuilder.CreateIndex(
                name: "IX_vendas_concluidas_FormaPagamento",
                table: "vendas_concluidas",
                column: "FormaPagamento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_mensagens");

            migrationBuilder.DropTable(
                name: "itens_vendas_concluidas");

            migrationBuilder.DropTable(
                name: "vendas_concluidas");
        }
    }
}
