using FrenteCaixa.Caixa.Application.Caixas.Interfaces;

namespace FrenteCaixa.Caixa.Tests;

public sealed class UnidadeTrabalhoEmMemoria : IUnidadeTrabalho
{
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
