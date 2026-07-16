using FrenteCaixa.Identidade.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Identidade.Infrastructure.Persistencia;

public sealed class IdentidadeDbContext : DbContext
{
    public IdentidadeDbContext(DbContextOptions<IdentidadeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(usuario => usuario.Id);

            entity.Property(usuario => usuario.Id).ValueGeneratedNever();
            entity.Property(usuario => usuario.Nome).HasMaxLength(160).IsRequired();
            entity.Property(usuario => usuario.Email).HasMaxLength(220).IsRequired();
            entity.Property(usuario => usuario.SenhaHash).HasMaxLength(260).IsRequired();
            entity.Property(usuario => usuario.Perfil).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(usuario => usuario.Ativo).IsRequired();
            entity.Property(usuario => usuario.CriadoEm).IsRequired();
            entity.Property(usuario => usuario.AtualizadoEm).IsRequired();

            entity.HasIndex(usuario => usuario.Email).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(refreshToken => refreshToken.Id);

            entity.Property(refreshToken => refreshToken.Id).ValueGeneratedNever();
            entity.Property(refreshToken => refreshToken.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(refreshToken => refreshToken.CriadoEm).IsRequired();
            entity.Property(refreshToken => refreshToken.ExpiraEm).IsRequired();
            entity.Property(refreshToken => refreshToken.EnderecoIpCriacao).HasMaxLength(80);
            entity.Property(refreshToken => refreshToken.EnderecoIpRevogacao).HasMaxLength(80);

            entity.HasIndex(refreshToken => refreshToken.TokenHash).IsUnique();
            entity.HasOne(refreshToken => refreshToken.Usuario)
                .WithMany()
                .HasForeignKey(refreshToken => refreshToken.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
