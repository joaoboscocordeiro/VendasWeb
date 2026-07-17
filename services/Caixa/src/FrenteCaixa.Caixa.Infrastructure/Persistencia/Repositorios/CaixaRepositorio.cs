using FrenteCaixa.Caixa.Application.Caixas.Repositorios;
using FrenteCaixa.Caixa.Domain.Caixas;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Caixa.Infrastructure.Persistencia.Repositorios;

public sealed class CaixaRepositorio : ICaixaRepositorio
{
    private readonly CaixaDbContext _dbContext;

    public CaixaRepositorio(CaixaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CaixaOperacional?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.CaixasOperacionais.FindAsync([id], cancellationToken);
    }

    public async Task<CaixaOperacional?> ObterCaixaAbertoPorOperadorAsync(
        Guid operadorId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.CaixasOperacionais
            .SingleOrDefaultAsync(
                caixa => caixa.OperadorId == operadorId && caixa.Status == StatusCaixa.Aberto,
                cancellationToken);
    }

    public async Task AdicionarCaixaAsync(CaixaOperacional caixa, CancellationToken cancellationToken)
    {
        await _dbContext.CaixasOperacionais.AddAsync(caixa, cancellationToken);
    }

    public async Task AdicionarMovimentacaoAsync(MovimentacaoCaixa movimentacao, CancellationToken cancellationToken)
    {
        await _dbContext.MovimentacoesCaixa.AddAsync(movimentacao, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MovimentacaoCaixa>> ListarMovimentacoesAsync(
        Guid caixaId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.MovimentacoesCaixa
            .AsNoTracking()
            .Where(movimentacao => movimentacao.CaixaId == caixaId)
            .OrderBy(movimentacao => movimentacao.CriadaEm)
            .ToArrayAsync(cancellationToken);
    }
}
