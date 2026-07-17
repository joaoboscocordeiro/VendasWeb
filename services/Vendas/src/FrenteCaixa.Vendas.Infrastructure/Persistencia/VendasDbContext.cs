using FrenteCaixa.BuildingBlocks.Outbox;
using FrenteCaixa.Vendas.Domain.Vendas;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Vendas.Infrastructure.Persistencia;

public sealed class VendasDbContext : DbContext
{
    public VendasDbContext(DbContextOptions<VendasDbContext> options)
        : base(options)
    {
    }

    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<RegistroOutbox> OutboxMensagens => Set<RegistroOutbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venda>(entity =>
        {
            entity.ToTable("vendas");
            entity.HasKey(venda => venda.Id);

            entity.Property(venda => venda.Id).ValueGeneratedNever();
            entity.Property(venda => venda.OperadorId).IsRequired();
            entity.Property(venda => venda.CaixaId).IsRequired();
            entity.Property(venda => venda.Status)
                .HasConversion<string>()
                .HasMaxLength(24)
                .IsRequired();
            entity.Property(venda => venda.CriadaEm).IsRequired();
            entity.Property(venda => venda.AtualizadaEm).IsRequired();

            entity.HasMany(venda => venda.Itens)
                .WithOne()
                .HasForeignKey(item => item.VendaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(venda => venda.Itens)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasIndex(venda => venda.OperadorId);
            entity.HasIndex(venda => venda.CaixaId);
        });

        modelBuilder.Entity<ItemVenda>(entity =>
        {
            entity.ToTable("itens_venda");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Id).ValueGeneratedNever();
            entity.Property(item => item.VendaId).IsRequired();
            entity.Property(item => item.ProdutoId).IsRequired();
            entity.Property(item => item.DescricaoProduto).HasMaxLength(220).IsRequired();
            entity.Property(item => item.Quantidade).HasPrecision(18, 3).IsRequired();
            entity.Property(item => item.PrecoUnitario).HasPrecision(18, 2).IsRequired();
            entity.Property(item => item.CriadoEm).IsRequired();

            entity.HasIndex(item => item.VendaId);
            entity.HasIndex(item => item.ProdutoId);
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
