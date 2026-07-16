using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

namespace FrenteCaixa.Identidade.Infrastructure.Persistencia;

public sealed class UnidadeTrabalho : IUnidadeTrabalho
{
    private readonly IdentidadeDbContext _contexto;

    public UnidadeTrabalho(IdentidadeDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return _contexto.SaveChangesAsync(cancellationToken);
    }
}
