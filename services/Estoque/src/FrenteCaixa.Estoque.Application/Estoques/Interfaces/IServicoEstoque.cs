using FrenteCaixa.Estoque.Application.Estoques.Contratos;

namespace FrenteCaixa.Estoque.Application.Estoques.Interfaces;

public interface IServicoEstoque
{
    Task<SaldoProdutoResponse> ObterSaldoAsync(Guid produtoId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MovimentacaoEstoqueResponse>> ListarMovimentacoesAsync(
        Guid produtoId,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<MovimentacaoEstoqueResponse>> RegistrarAjusteAsync(
        RegistrarAjusteEstoqueRequest request,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<IReadOnlyCollection<MovimentacaoEstoqueResponse>>> RegistrarDeducaoVendaAsync(
        DeducaoEstoqueRequest request,
        CancellationToken cancellationToken);
}
