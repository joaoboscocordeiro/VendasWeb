using FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;

namespace FrenteCaixa.CatalogoProdutos.Infrastructure.Persistencia;

public sealed class UnidadeTrabalho : IUnidadeTrabalho
{
    private readonly CatalogoProdutosDbContext _contexto;

    public UnidadeTrabalho(CatalogoProdutosDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
