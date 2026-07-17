using FrenteCaixa.Caixa.Application.Caixas.Interfaces;

namespace FrenteCaixa.Caixa.Infrastructure.Persistencia;

public sealed class UnidadeTrabalho : IUnidadeTrabalho
{
    private readonly CaixaDbContext _dbContext;

    public UnidadeTrabalho(CaixaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
