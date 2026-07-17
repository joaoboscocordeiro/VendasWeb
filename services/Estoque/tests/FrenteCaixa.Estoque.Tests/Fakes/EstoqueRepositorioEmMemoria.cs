using FrenteCaixa.Estoque.Application.Estoques.Repositorios;
using FrenteCaixa.Estoque.Domain.Estoques;

namespace FrenteCaixa.Estoque.Tests;

public sealed class EstoqueRepositorioEmMemoria : IEstoqueRepositorio
{
    private readonly BancoEstoqueEmMemoria _banco;

    public EstoqueRepositorioEmMemoria(BancoEstoqueEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<SaldoProduto?> ObterSaldoPorProdutoAsync(Guid produtoId, CancellationToken cancellationToken)
    {
        var saldo = _banco.Saldos.SingleOrDefault(item => item.ProdutoId == produtoId);

        return Task.FromResult(saldo);
    }

    public Task AdicionarSaldoAsync(SaldoProduto saldo, CancellationToken cancellationToken)
    {
        _banco.Saldos.Add(saldo);

        return Task.CompletedTask;
    }

    public Task AdicionarMovimentacaoAsync(MovimentacaoEstoque movimentacao, CancellationToken cancellationToken)
    {
        _banco.Movimentacoes.Add(movimentacao);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<MovimentacaoEstoque>> ListarMovimentacoesPorProdutoAsync(
        Guid produtoId,
        CancellationToken cancellationToken)
    {
        var movimentacoes = _banco.Movimentacoes
            .Where(movimentacao => movimentacao.ProdutoId == produtoId)
            .OrderByDescending(movimentacao => movimentacao.CriadaEm)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<MovimentacaoEstoque>>(movimentacoes);
    }
}
