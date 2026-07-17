using FrenteCaixa.BuildingBlocks.Inbox;
using FrenteCaixa.Relatorios.Domain.Relatorios;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Relatorios.Infrastructure.Persistencia;

public sealed class RelatoriosDbContext : DbContext
{
    public RelatoriosDbContext(DbContextOptions<RelatoriosDbContext> options)
        : base(options)
    {
    }

    public DbSet<VendaConcluidaProjetada> VendasConcluidas => Set<VendaConcluidaProjetada>();
    public DbSet<ItemVendaConcluidaProjetada> ItensVendasConcluidas => Set<ItemVendaConcluidaProjetada>();
    public DbSet<RegistroInbox> InboxMensagens => Set<RegistroInbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VendaConcluidaProjetada>(entity =>
        {
            entity.ToTable("vendas_concluidas");
            entity.HasKey(venda => venda.VendaId);

            entity.Property(venda => venda.VendaId).ValueGeneratedNever();
            entity.Property(venda => venda.CaixaId).IsRequired();
            entity.Property(venda => venda.OperadorId).IsRequired();
            entity.Property(venda => venda.PagamentoId).IsRequired();
            entity.Property(venda => venda.FormaPagamento).HasMaxLength(40).IsRequired();
            entity.Property(venda => venda.ValorTotal).HasPrecision(18, 2).IsRequired();
            entity.Property(venda => venda.ConcluidaEm).IsRequired();
            entity.Property(venda => venda.ProjetadaEm).IsRequired();

            entity.HasMany(venda => venda.Itens)
                .WithOne()
                .HasForeignKey(item => item.VendaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(venda => venda.Itens)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasIndex(venda => venda.ConcluidaEm);
            entity.HasIndex(venda => venda.FormaPagamento);
        });

        modelBuilder.Entity<ItemVendaConcluidaProjetada>(entity =>
        {
            entity.ToTable("itens_vendas_concluidas");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Id).ValueGeneratedNever();
            entity.Property(item => item.VendaId).IsRequired();
            entity.Property(item => item.ProdutoId).IsRequired();
            entity.Property(item => item.DescricaoProduto).HasMaxLength(220).IsRequired();
            entity.Property(item => item.Quantidade).HasPrecision(18, 3).IsRequired();
            entity.Property(item => item.PrecoUnitario).HasPrecision(18, 2).IsRequired();
            entity.Property(item => item.Subtotal).HasPrecision(18, 2).IsRequired();

            entity.HasIndex(item => item.VendaId);
            entity.HasIndex(item => item.ProdutoId);
        });

        modelBuilder.Entity<RegistroInbox>(entity =>
        {
            entity.ToTable("inbox_mensagens");
            entity.HasKey(mensagem => mensagem.MensagemId);

            entity.Property(mensagem => mensagem.MensagemId).ValueGeneratedNever();
            entity.Property(mensagem => mensagem.RoutingKey).HasMaxLength(160).IsRequired();
            entity.Property(mensagem => mensagem.ConsumidoEm).IsRequired();

            entity.HasIndex(mensagem => mensagem.RoutingKey);
        });
    }
}
