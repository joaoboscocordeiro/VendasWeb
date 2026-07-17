using FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;

namespace FrenteCaixa.CatalogoProdutos.Tests;

public sealed class UnidadeTrabalhoEmMemoria : IUnidadeTrabalho
{
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
