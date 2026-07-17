using FrenteCaixa.CatalogoProdutos.Domain.Produtos;

namespace FrenteCaixa.CatalogoProdutos.Application.Produtos.Repositorios;

public interface IProdutoRepositorio
{
    Task AdicionarAsync(Produto produto, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Produto>> ListarAsync(CancellationToken cancellationToken);

    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarrasEan, CancellationToken cancellationToken);
}
