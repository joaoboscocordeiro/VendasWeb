using FrenteCaixa.BuildingBlocks.Outbox;
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
    public DbSet<RegistroOutbox> OutboxMensagens => Set<RegistroOutbox>();

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

        modelBuilder.Entity<RegistroOutbox>(entity =>
        {
            entity.ToTable("outbox_mensagens");
            entity.HasKey(mensagem => mensagem.Id);

            entity.Property(mensagem => mensagem.Id).ValueGeneratedNever();
            entity.Property(mensagem => mensagem.RoutingKey).HasMaxLength(160).IsRequired();
            entity.Property(mensagem => mensagem.Tipo).HasMaxLength(120).IsRequired();
            entity.Property(mensagem => mensagem.Versao).IsRequired();
            entity.Property(mensagem => mensagem.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(mensagem => mensagem.CriadoEm).IsRequired();
            entity.Property(mensagem => mensagem.ProcessadoEm);
            entity.Property(mensagem => mensagem.Tentativas).IsRequired();
            entity.Property(mensagem => mensagem.UltimoErro).HasMaxLength(1000);

            entity.HasIndex(mensagem => mensagem.ProcessadoEm);
            entity.HasIndex(mensagem => mensagem.RoutingKey);
        });
    }
}
