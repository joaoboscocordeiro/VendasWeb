using FrenteCaixa.Relatorios.Domain.Relatorios;

namespace FrenteCaixa.Relatorios.Tests;

public sealed class BancoRelatoriosEmMemoria
{
    public List<VendaConcluidaProjetada> Vendas { get; } = [];
    public HashSet<Guid> MensagensProcessadas { get; } = [];

    public void Limpar()
    {
        Vendas.Clear();
        MensagensProcessadas.Clear();
    }
}
