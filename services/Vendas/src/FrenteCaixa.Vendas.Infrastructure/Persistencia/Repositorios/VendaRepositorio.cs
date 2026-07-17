using FrenteCaixa.Vendas.Application.Vendas.Repositorios;
using FrenteCaixa.Vendas.Domain.Vendas;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Vendas.Infrastructure.Persistencia.Repositorios;

public sealed class VendaRepositorio : IVendaRepositorio
{
    private readonly VendasDbContext _dbContext;

    public VendaRepositorio(VendasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Vendas
            .Include(venda => venda.Itens)
            .SingleOrDefaultAsync(venda => venda.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Venda venda, CancellationToken cancellationToken)
    {
        await _dbContext.Vendas.AddAsync(venda, cancellationToken);
    }
}
