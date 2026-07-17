using FrenteCaixa.Caixa.Application.Caixas.Repositorios;

namespace FrenteCaixa.Caixa.Tests;

public sealed class InboxRepositorioCaixaEmMemoria : IInboxRepositorioCaixa
{
    private readonly BancoCaixaEmMemoria _banco;

    public InboxRepositorioCaixaEmMemoria(BancoCaixaEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<bool> JaFoiProcessadaAsync(Guid mensagemId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_banco.MensagensProcessadas.Contains(mensagemId));
    }

    public Task RegistrarProcessamentoAsync(
        Guid mensagemId,
        string routingKey,
        DateTimeOffset processadaEm,
        CancellationToken cancellationToken)
    {
        _banco.MensagensProcessadas.Add(mensagemId);

        return Task.CompletedTask;
    }
}
