using FrenteCaixa.Caixa.Domain.Caixas;

namespace FrenteCaixa.Caixa.Tests;

public sealed class BancoCaixaEmMemoria
{
    public List<CaixaOperacional> Caixas { get; } = [];
    public List<MovimentacaoCaixa> Movimentacoes { get; } = [];
    public List<VendaCaixaProjetada> VendasProjetadas { get; } = [];
    public HashSet<Guid> MensagensProcessadas { get; } = [];

    public void Limpar()
    {
        Caixas.Clear();
        Movimentacoes.Clear();
        VendasProjetadas.Clear();
        MensagensProcessadas.Clear();
    }
}
