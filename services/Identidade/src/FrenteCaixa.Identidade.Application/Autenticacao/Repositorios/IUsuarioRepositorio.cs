using FrenteCaixa.Identidade.Domain.Usuarios;

namespace FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken);

    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
}
