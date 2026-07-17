using FrenteCaixa.Relatorios.Application.Relatorios.Interfaces;

namespace FrenteCaixa.Relatorios.Infrastructure.Persistencia;

public sealed class UnidadeTrabalho : IUnidadeTrabalho
{
    private readonly RelatoriosDbContext _dbContext;

    public UnidadeTrabalho(RelatoriosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
