using FrenteCaixa.CatalogoProdutos.Application.Produtos.Eventos;
using FrenteCaixa.CatalogoProdutos.Domain.Produtos;

namespace FrenteCaixa.CatalogoProdutos.Tests;

public sealed class RegistradorEventosProdutoEmMemoria : IRegistradorEventosProduto
{
    private readonly BancoCatalogoEmMemoria _banco;

    public RegistradorEventosProdutoEmMemoria(BancoCatalogoEmMemoria banco)
    {
        _banco = banco;
    }

    public Task RegistrarProdutoCriadoAsync(Produto produto, CancellationToken cancellationToken)
    {
        _banco.ProdutosCriadosComEvento.Add(produto);

        return Task.CompletedTask;
    }
}
