using FrenteCaixa.Estoque.Application.Estoques.Interfaces;

namespace FrenteCaixa.Estoque.Infrastructure.Tempo;

public sealed class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}
