using FrenteCaixa.Vendas.Application.Vendas.Repositorios;
using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Tests;

public sealed class VendaRepositorioEmMemoria : IVendaRepositorio
{
    private readonly BancoVendasEmMemoria _banco;

    public VendaRepositorioEmMemoria(BancoVendasEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var venda = _banco.Vendas.SingleOrDefault(item => item.Id == id);

        return Task.FromResult(venda);
    }

    public Task AdicionarAsync(Venda venda, CancellationToken cancellationToken)
    {
        _banco.Vendas.Add(venda);

        return Task.CompletedTask;
    }
}
