using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.BuildingBlocks.Outbox;

public sealed class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxBackgroundService> _logger;

    public OutboxBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxOptions> options,
        ILogger<OutboxBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Habilitado)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processador = scope.ServiceProvider.GetRequiredService<ProcessadorOutbox>();

                await processador.ProcessarPendentesAsync(_options.QuantidadePorLote, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Falha no ciclo de processamento Outbox");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.IntervaloSegundos), stoppingToken);
        }
    }
}
