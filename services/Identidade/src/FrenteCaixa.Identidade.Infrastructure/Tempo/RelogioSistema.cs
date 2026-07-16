using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

namespace FrenteCaixa.Identidade.Infrastructure.Tempo;

public sealed class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}
