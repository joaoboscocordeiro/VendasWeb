using FrenteCaixa.Caixa.Application.Caixas.Interfaces;

namespace FrenteCaixa.Caixa.Infrastructure.Tempo;

public sealed class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}
