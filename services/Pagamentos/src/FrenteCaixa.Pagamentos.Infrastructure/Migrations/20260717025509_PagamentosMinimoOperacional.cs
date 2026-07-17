using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrenteCaixa.Pagamentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PagamentosMinimoOperacional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendaId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormaPagamento = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ValorVenda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorPago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Troco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagamentos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_CriadoEm",
                table: "pagamentos",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_VendaId",
                table: "pagamentos",
                column: "VendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagamentos");
        }
    }
}
