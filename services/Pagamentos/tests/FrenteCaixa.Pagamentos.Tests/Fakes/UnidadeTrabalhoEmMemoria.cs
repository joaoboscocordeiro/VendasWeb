using FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;

namespace FrenteCaixa.Pagamentos.Tests;

public sealed class UnidadeTrabalhoEmMemoria : IUnidadeTrabalho
{
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
