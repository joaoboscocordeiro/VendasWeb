using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrenteCaixa.CatalogoProdutos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoProdutosCrud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    CodigoBarrasEan = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PrecoCusto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecoVenda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produtos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_produtos_CodigoBarrasEan",
                table: "produtos",
                column: "CodigoBarrasEan",
                unique: true,
                filter: "\"CodigoBarrasEan\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "produtos");
        }
    }
}
