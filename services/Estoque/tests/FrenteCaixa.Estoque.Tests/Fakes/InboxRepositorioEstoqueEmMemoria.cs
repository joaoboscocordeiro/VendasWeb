using FrenteCaixa.Estoque.Application.Estoques.Repositorios;

namespace FrenteCaixa.Estoque.Tests;

public sealed class InboxRepositorioEstoqueEmMemoria : IInboxRepositorioEstoque
{
    private readonly BancoEstoqueEmMemoria _banco;

    public InboxRepositorioEstoqueEmMemoria(BancoEstoqueEmMemoria banco)
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
        DateTimeOffset consumidoEm,
        CancellationToken cancellationToken)
    {
        _banco.MensagensProcessadas.Add(mensagemId);

        return Task.CompletedTask;
    }
}
