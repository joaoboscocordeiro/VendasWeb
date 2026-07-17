using FrenteCaixa.CatalogoProdutos.Application.Produtos.Contratos;

namespace FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;

public interface IServicoProdutos
{
    Task<ResultadoOperacao<ProdutoResponse>> CadastrarAsync(
        CadastrarProdutoRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProdutoResponse>> ListarAsync(CancellationToken cancellationToken);

    Task<ResultadoOperacao<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ResultadoOperacao<ProdutoResponse>> ObterPorCodigoBarrasAsync(
        string codigoBarrasEan,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<ProdutoResponse>> AtualizarAsync(
        Guid id,
        AtualizarProdutoRequest request,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<bool>> InativarAsync(Guid id, CancellationToken cancellationToken);
}
