using FrenteCaixa.CatalogoProdutos.Domain.Produtos;

namespace FrenteCaixa.CatalogoProdutos.Tests;

public sealed class BancoCatalogoEmMemoria
{
    public List<Produto> Produtos { get; } = new();

    public void Limpar()
    {
        Produtos.Clear();
    }
}
