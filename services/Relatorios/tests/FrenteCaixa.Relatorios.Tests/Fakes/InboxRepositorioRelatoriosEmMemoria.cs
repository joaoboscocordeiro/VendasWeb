using FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;

namespace FrenteCaixa.Relatorios.Tests;

public sealed class InboxRepositorioRelatoriosEmMemoria : IInboxRepositorioRelatorios
{
    private readonly BancoRelatoriosEmMemoria _banco;

    public InboxRepositorioRelatoriosEmMemoria(BancoRelatoriosEmMemoria banco)
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
