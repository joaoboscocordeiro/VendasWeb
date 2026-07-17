using FrenteCaixa.Identidade.Application.Autenticacao;
using FrenteCaixa.Identidade.Application.Usuarios.Contratos;

namespace FrenteCaixa.Identidade.Application.Usuarios.Interfaces;

public interface IServicoUsuarios
{
    Task<ResultadoOperacao<UsuarioResponse>> CadastrarAsync(
        CadastrarUsuarioRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UsuarioResponse>> ListarAsync(CancellationToken cancellationToken);

    Task<ResultadoOperacao<UsuarioResponse>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ResultadoOperacao<UsuarioResponse>> AtualizarAsync(
        Guid id,
        AtualizarUsuarioRequest request,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<bool>> InativarAsync(Guid id, CancellationToken cancellationToken);
}
