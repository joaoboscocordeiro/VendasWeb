using FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;

namespace FrenteCaixa.CatalogoProdutos.Tests;

public sealed class RelogioFixo : IRelogio
{
    public DateTimeOffset Agora { get; } = new(2026, 7, 16, 12, 0, 0, TimeSpan.Zero);
}
