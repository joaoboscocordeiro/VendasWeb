using FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;
using FrenteCaixa.Identidade.Domain.Usuarios;

namespace FrenteCaixa.Identidade.Tests;

public sealed class UsuarioRepositorioEmMemoria : IUsuarioRepositorio
{
    private readonly BancoIdentidadeEmMemoria _banco;

    public UsuarioRepositorioEmMemoria(BancoIdentidadeEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken)
    {
        var usuario = _banco.Usuarios.SingleOrDefault(usuario => usuario.Email == email);

        return Task.FromResult(usuario);
    }

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = _banco.Usuarios.SingleOrDefault(usuario => usuario.Id == id);

        return Task.FromResult(usuario);
    }
}
