using FrenteCaixa.CatalogoProdutos.Domain.Produtos;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.CatalogoProdutos.Infrastructure.Persistencia;

public sealed class CatalogoProdutosDbContext : DbContext
{
    public CatalogoProdutosDbContext(DbContextOptions<CatalogoProdutosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("produtos");
            entity.HasKey(produto => produto.Id);

            entity.Property(produto => produto.Id).ValueGeneratedNever();
            entity.Property(produto => produto.Descricao).HasMaxLength(220).IsRequired();
            entity.Property(produto => produto.CodigoBarrasEan).HasMaxLength(32);
            entity.Property(produto => produto.PrecoCusto).HasPrecision(18, 2).IsRequired();
            entity.Property(produto => produto.PrecoVenda).HasPrecision(18, 2).IsRequired();
            entity.Property(produto => produto.Ativo).IsRequired();
            entity.Property(produto => produto.CriadoEm).IsRequired();
            entity.Property(produto => produto.AtualizadoEm).IsRequired();

            entity.HasIndex(produto => produto.CodigoBarrasEan)
                .IsUnique()
                .HasFilter("\"CodigoBarrasEan\" IS NOT NULL");
        });
    }
}
