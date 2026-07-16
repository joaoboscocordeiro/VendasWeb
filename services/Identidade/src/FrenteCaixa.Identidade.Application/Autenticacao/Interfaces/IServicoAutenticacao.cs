using FrenteCaixa.Identidade.Application.Autenticacao.Contratos;

namespace FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

public interface IServicoAutenticacao
{
    Task<ResultadoOperacao<AutenticacaoResponse>> EntrarAsync(LoginRequest request, string? enderecoIp, CancellationToken cancellationToken);

    Task<ResultadoOperacao<AutenticacaoResponse>> RenovarAsync(RenovarTokenRequest request, string? enderecoIp, CancellationToken cancellationToken);

    Task<ResultadoOperacao<bool>> SairAsync(LogoutRequest request, string? enderecoIp, CancellationToken cancellationToken);

    Task<ResultadoOperacao<UsuarioAutenticadoResponse>> ObterUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken);
}
