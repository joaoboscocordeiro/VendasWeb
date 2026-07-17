using FrenteCaixa.Caixa.Domain.Caixas;
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
    }
}
