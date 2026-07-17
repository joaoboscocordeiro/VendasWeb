using FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;

namespace FrenteCaixa.Pagamentos.Infrastructure.Tempo;

public sealed class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}
