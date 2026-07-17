using FrenteCaixa.BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.CatalogoProdutos.Infrastructure.Persistencia.Repositorios;

public sealed class OutboxRepositorio : IRepositorioOutbox
{
    private readonly CatalogoProdutosDbContext _dbContext;

    public OutboxRepositorio(CatalogoProdutosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<RegistroOutbox>> ObterPendentesAsync(
        int quantidade,
        CancellationToken cancellationToken)
    {
        return await _dbContext.OutboxMensagens
            .Where(mensagem => mensagem.ProcessadoEm == null)
            .OrderBy(mensagem => mensagem.CriadoEm)
            .Take(quantidade)
            .ToArrayAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
