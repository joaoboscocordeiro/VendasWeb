using FrenteCaixa.CatalogoProdutos.Application.Produtos.Repositorios;
using FrenteCaixa.CatalogoProdutos.Domain.Produtos;

namespace FrenteCaixa.CatalogoProdutos.Tests;

public sealed class ProdutoRepositorioEmMemoria : IProdutoRepositorio
{
    private readonly BancoCatalogoEmMemoria _banco;

    public ProdutoRepositorioEmMemoria(BancoCatalogoEmMemoria banco)
    {
        _banco = banco;
    }

    public Task AdicionarAsync(Produto produto, CancellationToken cancellationToken)
    {
        _banco.Produtos.Add(produto);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Produto>> ListarAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Produto> produtos = _banco.Produtos
            .OrderBy(produto => produto.Descricao)
            .ThenBy(produto => produto.CodigoBarrasEan)
            .ToArray();

        return Task.FromResult(produtos);
    }

    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var produto = _banco.Produtos.SingleOrDefault(produto => produto.Id == id);

        return Task.FromResult(produto);
    }

    public Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarrasEan, CancellationToken cancellationToken)
    {
        var produto = _banco.Produtos.SingleOrDefault(produto => produto.CodigoBarrasEan == codigoBarrasEan);

        return Task.FromResult(produto);
    }
}
