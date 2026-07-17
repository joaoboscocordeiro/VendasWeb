using FrenteCaixa.Vendas.Application.Vendas.Interfaces;

namespace FrenteCaixa.Vendas.Tests;

public sealed class UnidadeTrabalhoEmMemoria : IUnidadeTrabalho
{
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
