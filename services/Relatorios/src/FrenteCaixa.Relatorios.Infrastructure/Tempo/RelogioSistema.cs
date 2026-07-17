using FrenteCaixa.Relatorios.Application.Relatorios.Interfaces;

namespace FrenteCaixa.Relatorios.Infrastructure.Tempo;

public sealed class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}
