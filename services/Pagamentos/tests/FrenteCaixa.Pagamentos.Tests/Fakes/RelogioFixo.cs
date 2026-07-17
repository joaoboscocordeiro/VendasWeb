using FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;

namespace FrenteCaixa.Pagamentos.Tests;

public sealed class RelogioFixo : IRelogio
{
    public DateTimeOffset Agora => new(2026, 7, 17, 15, 0, 0, TimeSpan.Zero);
}
