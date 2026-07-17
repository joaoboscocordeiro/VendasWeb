using FrenteCaixa.Pagamentos.Application.Pagamentos.Repositorios;
using FrenteCaixa.Pagamentos.Domain.Pagamentos;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Pagamentos.Infrastructure.Persistencia.Repositorios;

public sealed class PagamentoRepositorio : IPagamentoRepositorio
{
    private readonly PagamentosDbContext _dbContext;

    public PagamentoRepositorio(PagamentosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Pagamentos.SingleOrDefaultAsync(
            pagamento => pagamento.Id == id,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Pagamento>> ListarPorVendaAsync(
        Guid vendaId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Pagamentos
            .Where(pagamento => pagamento.VendaId == vendaId)
            .OrderBy(pagamento => pagamento.CriadoEm)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Pagamento pagamento, CancellationToken cancellationToken)
    {
        await _dbContext.Pagamentos.AddAsync(pagamento, cancellationToken);
    }
}
