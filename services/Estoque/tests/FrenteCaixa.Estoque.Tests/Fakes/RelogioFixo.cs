using FrenteCaixa.Estoque.Application.Estoques.Interfaces;

namespace FrenteCaixa.Estoque.Tests;

public sealed class RelogioFixo : IRelogio
{
    public DateTimeOffset Agora => new(2026, 7, 17, 12, 0, 0, TimeSpan.Zero);
}
