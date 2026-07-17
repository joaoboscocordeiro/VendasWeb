using FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;

namespace FrenteCaixa.Pagamentos.Infrastructure.Persistencia;

public sealed class UnidadeTrabalho : IUnidadeTrabalho
{
    private readonly PagamentosDbContext _dbContext;

    public UnidadeTrabalho(PagamentosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
