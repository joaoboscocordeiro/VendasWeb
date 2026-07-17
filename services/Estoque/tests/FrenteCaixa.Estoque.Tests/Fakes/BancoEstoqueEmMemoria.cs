using FrenteCaixa.Estoque.Domain.Estoques;

namespace FrenteCaixa.Estoque.Tests;

public sealed class BancoEstoqueEmMemoria
{
    public List<SaldoProduto> Saldos { get; } = [];
    public List<MovimentacaoEstoque> Movimentacoes { get; } = [];
    public HashSet<Guid> MensagensProcessadas { get; } = [];

    public void Limpar()
    {
        Saldos.Clear();
        Movimentacoes.Clear();
        MensagensProcessadas.Clear();
    }
}
