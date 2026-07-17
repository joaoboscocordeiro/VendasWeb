using FrenteCaixa.Vendas.Application.Vendas.Interfaces;

namespace FrenteCaixa.Vendas.Tests;

public sealed class RelogioFixo : IRelogio
{
    public DateTimeOffset Agora => new(2026, 7, 17, 14, 0, 0, TimeSpan.Zero);
}
