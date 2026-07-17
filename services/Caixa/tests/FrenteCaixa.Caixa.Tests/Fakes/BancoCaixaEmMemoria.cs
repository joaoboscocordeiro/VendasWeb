using FrenteCaixa.Caixa.Domain.Caixas;

namespace FrenteCaixa.Caixa.Tests;

public sealed class BancoCaixaEmMemoria
{
    public List<CaixaOperacional> Caixas { get; } = [];
    public List<MovimentacaoCaixa> Movimentacoes { get; } = [];

    public void Limpar()
    {
        Caixas.Clear();
        Movimentacoes.Clear();
    }
}
