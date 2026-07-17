using FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;
using FrenteCaixa.Relatorios.Domain.Relatorios;

namespace FrenteCaixa.Relatorios.Tests;

public sealed class RelatoriosRepositorioEmMemoria : IRelatoriosRepositorio
{
    private readonly BancoRelatoriosEmMemoria _banco;

    public RelatoriosRepositorioEmMemoria(BancoRelatoriosEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<VendaConcluidaProjetada?> ObterVendaAsync(
        Guid vendaId,
        CancellationToken cancellationToken)
    {
        var venda = _banco.Vendas.SingleOrDefault(item => item.VendaId == vendaId);

        return Task.FromResult(venda);
    }

    public Task AdicionarVendaAsync(VendaConcluidaProjetada venda, CancellationToken cancellationToken)
    {
        _banco.Vendas.Add(venda);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<VendaConcluidaProjetada>> ListarVendasAsync(
        CancellationToken cancellationToken)
    {
        var vendas = _banco.Vendas
            .OrderByDescending(venda => venda.ConcluidaEm)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<VendaConcluidaProjetada>>(vendas);
    }
}
