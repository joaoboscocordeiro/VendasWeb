using FrenteCaixa.Relatorios.Application.Relatorios.Interfaces;

namespace FrenteCaixa.Relatorios.Tests;

public sealed class UnidadeTrabalhoEmMemoria : IUnidadeTrabalho
{
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
