using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

namespace FrenteCaixa.Identidade.Tests;

public sealed class UnidadeTrabalhoEmMemoria : IUnidadeTrabalho
{
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
