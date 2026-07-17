using FrenteCaixa.Caixa.Domain.Caixas;
using FrenteCaixa.BuildingBlocks.Inbox;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Caixa.Infrastructure.Persistencia;

public sealed class CaixaDbContext : DbContext
{
    public CaixaDbContext(DbContextOptions<CaixaDbContext> options)
        : base(options)
    {
    }

    public DbSet<CaixaOperacional> CaixasOperacionais => Set<CaixaOperacional>();
    public DbSet<MovimentacaoCaixa> MovimentacoesCaixa => Set<MovimentacaoCaixa>();
    public DbSet<VendaCaixaProjetada> VendasCaixaProjetadas => Set<VendaCaixaProjetada>();
    public DbSet<RegistroInbox> InboxMensagens => Set<RegistroInbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaixaOperacional>(entity =>
        {
            entity.ToTable("caixas_operacionais");
            entity.HasKey(caixa => caixa.Id);

            entity.Property(caixa => caixa.Id).ValueGeneratedNever();
            entity.Property(caixa => caixa.OperadorId).IsRequired();
            entity.Property(caixa => caixa.ValorInicial).HasPrecision(18, 2).IsRequired();
            entity.Property(caixa => caixa.ValorFechamento).HasPrecision(18, 2);
            entity.Property(caixa => caixa.Status)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            entity.Property(caixa => caixa.AbertoEm).IsRequired();
            entity.Property(caixa => caixa.FechadoEm);
            entity.Property(caixa => caixa.AtualizadoEm).IsRequired();

            entity.HasIndex(caixa => caixa.OperadorId);
        });

        modelBuilder.Entity<MovimentacaoCaixa>(entity =>
        {
            entity.ToTable("movimentacoes_caixa");
            entity.HasKey(movimentacao => movimentacao.Id);

            entity.Property(movimentacao => movimentacao.Id).ValueGeneratedNever();
            entity.Property(movimentacao => movimentacao.CaixaId).IsRequired();
            entity.Property(movimentacao => movimentacao.OperadorId).IsRequired();
            entity.Property(movimentacao => movimentacao.Tipo)
                .HasConversion<string>()
                .HasMaxLength(24)
                .IsRequired();
            entity.Property(movimentacao => movimentacao.Valor).HasPrecision(18, 2).IsRequired();
            entity.Property(movimentacao => movimentacao.Descricao).HasMaxLength(180).IsRequired();
            entity.Property(movimentacao => movimentacao.CriadaEm).IsRequired();

            entity.HasIndex(movimentacao => movimentacao.CaixaId);
        });

        modelBuilder.Entity<VendaCaixaProjetada>(entity =>
        {
            entity.ToTable("vendas_caixa_projetadas");
            entity.HasKey(venda => venda.VendaId);

            entity.Property(venda => venda.VendaId).ValueGeneratedNever();
            entity.Property(venda => venda.CaixaId).IsRequired();
            entity.Property(venda => venda.OperadorId).IsRequired();
            entity.Property(venda => venda.PagamentoId).IsRequired();
            entity.Property(venda => venda.FormaPagamento).HasMaxLength(24).IsRequired();
            entity.Property(venda => venda.ValorTotal).HasPrecision(18, 2).IsRequired();
            entity.Property(venda => venda.OcorridaEm).IsRequired();
            entity.Property(venda => venda.ProjetadaEm).IsRequired();

            entity.HasIndex(venda => venda.CaixaId);
            entity.HasIndex(venda => venda.OperadorId);
        });

        modelBuilder.Entity<RegistroInbox>(entity =>
        {
            entity.ToTable("inbox_mensagens");
            entity.HasKey(mensagem => mensagem.MensagemId);

            entity.Property(mensagem => mensagem.MensagemId).ValueGeneratedNever();
            entity.Property(mensagem => mensagem.RoutingKey).HasMaxLength(180).IsRequired();
            entity.Property(mensagem => mensagem.ConsumidoEm).IsRequired();
        });
    }
}
