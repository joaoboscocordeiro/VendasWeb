using FrenteCaixa.CatalogoProdutos.Application.Produtos.Repositorios;
using FrenteCaixa.CatalogoProdutos.Domain.Produtos;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.CatalogoProdutos.Infrastructure.Persistencia.Repositorios;

public sealed class ProdutoRepositorio : IProdutoRepositorio
{
    private readonly CatalogoProdutosDbContext _contexto;

    public ProdutoRepositorio(CatalogoProdutosDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task AdicionarAsync(Produto produto, CancellationToken cancellationToken)
    {
        await _contexto.Produtos.AddAsync(produto, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Produto>> ListarAsync(CancellationToken cancellationToken)
    {
        return await _contexto.Produtos
            .OrderBy(produto => produto.Descricao)
            .ThenBy(produto => produto.CodigoBarrasEan)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _contexto.Produtos.SingleOrDefaultAsync(produto => produto.Id == id, cancellationToken);
    }

    public Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarrasEan, CancellationToken cancellationToken)
    {
        return _contexto.Produtos.SingleOrDefaultAsync(
            produto => produto.CodigoBarrasEan == codigoBarrasEan,
            cancellationToken);
    }
}
