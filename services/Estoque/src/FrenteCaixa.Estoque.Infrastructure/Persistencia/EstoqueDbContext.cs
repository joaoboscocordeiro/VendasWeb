using FrenteCaixa.BuildingBlocks.Inbox;
using FrenteCaixa.Estoque.Domain.Estoques;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Estoque.Infrastructure.Persistencia;

public sealed class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options)
        : base(options)
    {
    }

    public DbSet<SaldoProduto> SaldosProdutos => Set<SaldoProduto>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();
    public DbSet<RegistroInbox> InboxMensagens => Set<RegistroInbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SaldoProduto>(entity =>
        {
            entity.ToTable("saldos_produtos");
            entity.HasKey(saldo => saldo.ProdutoId);

            entity.Property(saldo => saldo.ProdutoId).ValueGeneratedNever();
            entity.Property(saldo => saldo.QuantidadeDisponivel).HasPrecision(18, 3).IsRequired();
            entity.Property(saldo => saldo.AtualizadoEm).IsRequired();
        });

        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.ToTable("movimentacoes_estoque");
            entity.HasKey(movimentacao => movimentacao.Id);

            entity.Property(movimentacao => movimentacao.Id).ValueGeneratedNever();
            entity.Property(movimentacao => movimentacao.ProdutoId).IsRequired();
            entity.Property(movimentacao => movimentacao.Tipo)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            entity.Property(movimentacao => movimentacao.Quantidade).HasPrecision(18, 3).IsRequired();
            entity.Property(movimentacao => movimentacao.QuantidadeAnterior).HasPrecision(18, 3).IsRequired();
            entity.Property(movimentacao => movimentacao.QuantidadeAtual).HasPrecision(18, 3).IsRequired();
            entity.Property(movimentacao => movimentacao.Motivo).HasMaxLength(300).IsRequired();
            entity.Property(movimentacao => movimentacao.CriadaEm).IsRequired();

            entity.HasIndex(movimentacao => movimentacao.ProdutoId);
        });

        modelBuilder.Entity<RegistroInbox>(entity =>
        {
            entity.ToTable("inbox_mensagens");
            entity.HasKey(registro => registro.MensagemId);

            entity.Property(registro => registro.MensagemId).ValueGeneratedNever();
            entity.Property(registro => registro.RoutingKey).HasMaxLength(160).IsRequired();
            entity.Property(registro => registro.ConsumidoEm).IsRequired();

            entity.HasIndex(registro => registro.RoutingKey);
        });
    }
}
