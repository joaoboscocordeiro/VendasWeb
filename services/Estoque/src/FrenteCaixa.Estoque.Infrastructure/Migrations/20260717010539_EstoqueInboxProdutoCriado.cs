using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrenteCaixa.Estoque.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EstoqueInboxProdutoCriado : Migration
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

            migrationBuilder.CreateIndex(
                name: "IX_inbox_mensagens_RoutingKey",
                table: "inbox_mensagens",
                column: "RoutingKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_mensagens");
        }
    }
}
