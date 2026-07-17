namespace FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;

public interface IUnidadeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
