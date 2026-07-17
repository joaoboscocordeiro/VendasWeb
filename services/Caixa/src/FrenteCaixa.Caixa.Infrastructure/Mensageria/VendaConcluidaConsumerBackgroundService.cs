using System.Text.Json;
using FrenteCaixa.BuildingBlocks.Mensageria;
using FrenteCaixa.Caixa.Application.Caixas.Eventos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FrenteCaixa.Caixa.Infrastructure.Mensageria;

public sealed class VendaConcluidaConsumerBackgroundService : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly VendaConcluidaConsumerOptions _consumerOptions;
    private readonly ILogger<VendaConcluidaConsumerBackgroundService> _logger;

    public VendaConcluidaConsumerBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        IOptions<VendaConcluidaConsumerOptions> consumerOptions,
        ILogger<VendaConcluidaConsumerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _consumerOptions = consumerOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_consumerOptions.Habilitado)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumirAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Falha ao consumir vendas.venda-concluida.v1 no Caixa. Nova tentativa em 10 segundos.");

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task ConsumirAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.HostName,
            Port = _rabbitMqOptions.Port,
            UserName = _rabbitMqOptions.UserName,
            Password = _rabbitMqOptions.Password,
            VirtualHost = _rabbitMqOptions.VirtualHost
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: _rabbitMqOptions.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: _consumerOptions.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: _consumerOptions.QueueName,
            exchange: _rabbitMqOptions.ExchangeName,
            routingKey: _consumerOptions.RoutingKey,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += ProcessarEntregaAsync;

        await channel.BasicConsumeAsync(
            queue: _consumerOptions.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);

        async Task ProcessarEntregaAsync(object sender, BasicDeliverEventArgs entrega)
        {
            try
            {
                if (!Guid.TryParse(entrega.BasicProperties.MessageId, out var mensagemId))
                {
                    _logger.LogWarning(
                        "Mensagem {RoutingKey} descartada por MessageId ausente ou invalido.",
                        entrega.RoutingKey);

                    await channel.BasicNackAsync(
                        deliveryTag: entrega.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);
                    return;
                }

                var evento = JsonSerializer.Deserialize<VendaConcluidaEvento>(
                    entrega.Body.Span,
                    JsonOptions);

                if (evento is null)
                {
                    await channel.BasicNackAsync(
                        deliveryTag: entrega.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var processador = scope.ServiceProvider.GetRequiredService<IProcessadorVendaConcluidaCaixa>();

                await processador.ProcessarAsync(
                    mensagemId,
                    entrega.RoutingKey,
                    evento,
                    stoppingToken);

                await channel.BasicAckAsync(
                    deliveryTag: entrega.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Falha ao processar mensagem {RoutingKey} no Caixa.",
                    entrega.RoutingKey);

                await channel.BasicNackAsync(
                    deliveryTag: entrega.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        }
    }
}
