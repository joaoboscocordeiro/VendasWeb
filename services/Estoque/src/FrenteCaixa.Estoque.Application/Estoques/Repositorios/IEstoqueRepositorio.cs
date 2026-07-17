using FrenteCaixa.Estoque.Domain.Estoques;

namespace FrenteCaixa.Estoque.Application.Estoques.Repositorios;

public interface IEstoqueRepositorio
{
    Task<SaldoProduto?> ObterSaldoPorProdutoAsync(Guid produtoId, CancellationToken cancellationToken);

    Task AdicionarSaldoAsync(SaldoProduto saldo, CancellationToken cancellationToken);

    Task AdicionarMovimentacaoAsync(MovimentacaoEstoque movimentacao, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MovimentacaoEstoque>> ListarMovimentacoesPorProdutoAsync(
        Guid produtoId,
        CancellationToken cancellationToken);
}
