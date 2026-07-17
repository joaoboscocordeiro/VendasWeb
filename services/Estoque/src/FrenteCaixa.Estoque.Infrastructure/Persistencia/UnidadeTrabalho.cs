using FrenteCaixa.Estoque.Application.Estoques.Interfaces;

namespace FrenteCaixa.Estoque.Infrastructure.Persistencia;

public sealed class UnidadeTrabalho : IUnidadeTrabalho
{
    private readonly EstoqueDbContext _dbContext;

    public UnidadeTrabalho(EstoqueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
