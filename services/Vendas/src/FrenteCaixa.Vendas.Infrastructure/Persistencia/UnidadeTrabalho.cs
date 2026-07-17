using FrenteCaixa.Vendas.Application.Vendas.Interfaces;

namespace FrenteCaixa.Vendas.Infrastructure.Persistencia;

public sealed class UnidadeTrabalho : IUnidadeTrabalho
{
    private readonly VendasDbContext _dbContext;

    public UnidadeTrabalho(VendasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
