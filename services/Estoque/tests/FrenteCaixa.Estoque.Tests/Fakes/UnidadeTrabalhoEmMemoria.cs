using FrenteCaixa.Estoque.Application.Estoques.Interfaces;

namespace FrenteCaixa.Estoque.Tests;

public sealed class UnidadeTrabalhoEmMemoria : IUnidadeTrabalho
{
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
