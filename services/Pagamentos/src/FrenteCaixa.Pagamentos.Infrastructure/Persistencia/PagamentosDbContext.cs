using FrenteCaixa.Pagamentos.Domain.Pagamentos;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Pagamentos.Infrastructure.Persistencia;

public sealed class PagamentosDbContext : DbContext
{
    public PagamentosDbContext(DbContextOptions<PagamentosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pagamento>(entity =>
        {
            entity.ToTable("pagamentos");
            entity.HasKey(pagamento => pagamento.Id);

            entity.Property(pagamento => pagamento.Id).ValueGeneratedNever();
            entity.Property(pagamento => pagamento.VendaId).IsRequired();
            entity.Property(pagamento => pagamento.FormaPagamento)
                .HasConversion<string>()
                .HasMaxLength(24)
                .IsRequired();
            entity.Property(pagamento => pagamento.ValorVenda).HasPrecision(18, 2).IsRequired();
            entity.Property(pagamento => pagamento.ValorPago).HasPrecision(18, 2).IsRequired();
            entity.Property(pagamento => pagamento.Troco).HasPrecision(18, 2).IsRequired();
            entity.Property(pagamento => pagamento.Status)
                .HasConversion<string>()
                .HasMaxLength(24)
                .IsRequired();
            entity.Property(pagamento => pagamento.CriadoEm).IsRequired();

            entity.HasIndex(pagamento => pagamento.VendaId);
            entity.HasIndex(pagamento => pagamento.CriadoEm);
        });
    }
}
