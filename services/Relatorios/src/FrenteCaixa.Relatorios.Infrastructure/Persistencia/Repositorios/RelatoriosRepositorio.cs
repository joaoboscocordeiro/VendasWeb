using FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;
using FrenteCaixa.Relatorios.Domain.Relatorios;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Relatorios.Infrastructure.Persistencia.Repositorios;

public sealed class RelatoriosRepositorio : IRelatoriosRepositorio
{
    private readonly RelatoriosDbContext _dbContext;

    public RelatoriosRepositorio(RelatoriosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<VendaConcluidaProjetada?> ObterVendaAsync(
        Guid vendaId,
        CancellationToken cancellationToken)
    {
        return _dbContext.VendasConcluidas
            .Include(venda => venda.Itens)
            .SingleOrDefaultAsync(venda => venda.VendaId == vendaId, cancellationToken);
    }

    public async Task AdicionarVendaAsync(
        VendaConcluidaProjetada venda,
        CancellationToken cancellationToken)
    {
        await _dbContext.VendasConcluidas.AddAsync(venda, cancellationToken);
    }

    public async Task<IReadOnlyCollection<VendaConcluidaProjetada>> ListarVendasAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.VendasConcluidas
            .Include(venda => venda.Itens)
            .OrderByDescending(venda => venda.ConcluidaEm)
            .ToArrayAsync(cancellationToken);
    }
}
