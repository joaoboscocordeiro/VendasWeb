using FrenteCaixa.Vendas.Application.Vendas.Interfaces;

namespace FrenteCaixa.Vendas.Infrastructure.Tempo;

public sealed class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}
