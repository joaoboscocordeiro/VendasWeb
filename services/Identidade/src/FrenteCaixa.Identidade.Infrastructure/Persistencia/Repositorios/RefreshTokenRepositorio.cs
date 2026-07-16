using FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;
using FrenteCaixa.Identidade.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Identidade.Infrastructure.Persistencia.Repositorios;

public sealed class RefreshTokenRepositorio : IRefreshTokenRepositorio
{
    private readonly IdentidadeDbContext _contexto;

    public RefreshTokenRepositorio(IdentidadeDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<RefreshToken?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return _contexto.RefreshTokens
            .Include(refreshToken => refreshToken.Usuario)
            .SingleOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);
    }

    public async Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await _contexto.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }
}
