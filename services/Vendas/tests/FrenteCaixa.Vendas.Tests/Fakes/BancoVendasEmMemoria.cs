using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Tests;

public sealed class BancoVendasEmMemoria
{
    public List<Venda> Vendas { get; } = [];

    public void Limpar()
    {
        Vendas.Clear();
    }
}
