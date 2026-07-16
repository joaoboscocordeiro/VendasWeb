namespace FrenteCaixa.BuildingBlocks.Outbox;

public sealed class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}
