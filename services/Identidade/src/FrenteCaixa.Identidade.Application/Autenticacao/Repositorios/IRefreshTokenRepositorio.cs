using FrenteCaixa.Identidade.Domain.Usuarios;

namespace FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;

public interface IRefreshTokenRepositorio
{
    Task<RefreshToken?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
}
