using FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;
using FrenteCaixa.Identidade.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Identidade.Infrastructure.Persistencia.Repositorios;

public sealed class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly IdentidadeDbContext _contexto;

    public UsuarioRepositorio(IdentidadeDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _contexto.Usuarios.SingleOrDefaultAsync(usuario => usuario.Email == email, cancellationToken);
    }

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _contexto.Usuarios.SingleOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Usuario>> ListarAsync(CancellationToken cancellationToken)
    {
        return await _contexto.Usuarios
            .OrderBy(usuario => usuario.Nome)
            .ThenBy(usuario => usuario.Email)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        await _contexto.Usuarios.AddAsync(usuario, cancellationToken);
    }
}
