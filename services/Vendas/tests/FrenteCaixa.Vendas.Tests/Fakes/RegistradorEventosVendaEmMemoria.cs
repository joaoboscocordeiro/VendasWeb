using FrenteCaixa.Vendas.Application.Vendas.Eventos;
using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Tests;

public sealed class RegistradorEventosVendaEmMemoria : IRegistradorEventosVenda
{
    private readonly BancoVendasEmMemoria _banco;

    public RegistradorEventosVendaEmMemoria(BancoVendasEmMemoria banco)
    {
        _banco = banco;
    }

    public Task RegistrarVendaConcluidaAsync(
        Venda venda,
        Guid pagamentoId,
        string formaPagamento,
        CancellationToken cancellationToken)
    {
        _banco.OutboxRoutingKeys.Add("vendas.venda-concluida.v1");

        return Task.CompletedTask;
    }
}
