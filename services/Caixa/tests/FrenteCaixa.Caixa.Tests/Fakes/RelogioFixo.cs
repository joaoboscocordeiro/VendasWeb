using FrenteCaixa.Caixa.Application.Caixas.Interfaces;

namespace FrenteCaixa.Caixa.Tests;

public sealed class RelogioFixo : IRelogio
{
    public DateTimeOffset Agora => new(2026, 7, 17, 13, 0, 0, TimeSpan.Zero);
}
