using FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;
using FrenteCaixa.Identidade.Domain.Usuarios;

namespace FrenteCaixa.Identidade.Tests;

public sealed class RefreshTokenRepositorioEmMemoria : IRefreshTokenRepositorio
{
    private readonly BancoIdentidadeEmMemoria _banco;

    public RefreshTokenRepositorioEmMemoria(BancoIdentidadeEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<RefreshToken?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var refreshToken = _banco.RefreshTokens.SingleOrDefault(refreshToken => refreshToken.TokenHash == tokenHash);

        return Task.FromResult(refreshToken);
    }

    public Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        _banco.RefreshTokens.Add(refreshToken);

        return Task.CompletedTask;
    }
}
