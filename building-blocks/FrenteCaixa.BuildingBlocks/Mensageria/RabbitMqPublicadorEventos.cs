using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FrenteCaixa.BuildingBlocks.Mensageria;

public sealed class RabbitMqPublicadorEventos : IPublicadorEventos
{
    private readonly RabbitMqOptions _options;

    public RabbitMqPublicadorEventos(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublicarAsync(EnvelopeEventoIntegracao evento, CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            MessageId = evento.Id.ToString(),
            Type = evento.Tipo,
            CorrelationId = evento.CorrelationId,
            Timestamp = new AmqpTimestamp(evento.OcorridoEm.ToUnixTimeSeconds()),
            DeliveryMode = DeliveryModes.Persistent
        };

        var body = Encoding.UTF8.GetBytes(evento.PayloadJson);

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: evento.RoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
