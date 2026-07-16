namespace FrenteCaixa.BuildingBlocks.Inbox;

public sealed class RegistroInbox
{
    private RegistroInbox()
    {
        RoutingKey = string.Empty;
    }

    private RegistroInbox(Guid mensagemId, string routingKey, DateTimeOffset consumidoEm)
    {
        MensagemId = mensagemId;
        RoutingKey = routingKey;
        ConsumidoEm = consumidoEm;
    }

    public Guid MensagemId { get; private set; }

    public string RoutingKey { get; private set; }

    public DateTimeOffset ConsumidoEm { get; private set; }

    public static RegistroInbox Criar(Guid mensagemId, string routingKey, DateTimeOffset consumidoEm)
    {
        return new RegistroInbox(mensagemId, routingKey, consumidoEm);
    }
}
