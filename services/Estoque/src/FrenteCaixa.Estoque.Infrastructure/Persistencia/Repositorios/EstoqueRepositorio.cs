using FrenteCaixa.Estoque.Application.Estoques.Repositorios;
using FrenteCaixa.Estoque.Domain.Estoques;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Estoque.Infrastructure.Persistencia.Repositorios;

public sealed class EstoqueRepositorio : IEstoqueRepositorio
{
    private readonly EstoqueDbContext _dbContext;

    public EstoqueRepositorio(EstoqueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SaldoProduto?> ObterSaldoPorProdutoAsync(Guid produtoId, CancellationToken cancellationToken)
    {
        return await _dbContext.SaldosProdutos.FindAsync([produtoId], cancellationToken);
    }

    public async Task AdicionarSaldoAsync(SaldoProduto saldo, CancellationToken cancellationToken)
    {
        await _dbContext.SaldosProdutos.AddAsync(saldo, cancellationToken);
    }

    public async Task AdicionarMovimentacaoAsync(MovimentacaoEstoque movimentacao, CancellationToken cancellationToken)
    {
        await _dbContext.MovimentacoesEstoque.AddAsync(movimentacao, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MovimentacaoEstoque>> ListarMovimentacoesPorProdutoAsync(
        Guid produtoId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.MovimentacoesEstoque
            .AsNoTracking()
            .Where(movimentacao => movimentacao.ProdutoId == produtoId)
            .OrderByDescending(movimentacao => movimentacao.CriadaEm)
            .ToArrayAsync(cancellationToken);
    }
}
