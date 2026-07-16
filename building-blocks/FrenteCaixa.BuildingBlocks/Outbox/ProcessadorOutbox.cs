using FrenteCaixa.BuildingBlocks.Mensageria;
using Microsoft.Extensions.Logging;

namespace FrenteCaixa.BuildingBlocks.Outbox;

public sealed class ProcessadorOutbox
{
    private readonly IRepositorioOutbox _repositorio;
    private readonly IPublicadorEventos _publicador;
    private readonly IRelogio _relogio;
    private readonly ILogger<ProcessadorOutbox> _logger;

    public ProcessadorOutbox(
        IRepositorioOutbox repositorio,
        IPublicadorEventos publicador,
        IRelogio relogio,
        ILogger<ProcessadorOutbox> logger)
    {
        _repositorio = repositorio;
        _publicador = publicador;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<int> ProcessarPendentesAsync(int quantidade, CancellationToken cancellationToken)
    {
        var mensagens = await _repositorio.ObterPendentesAsync(quantidade, cancellationToken);
        var publicadas = 0;

        foreach (var mensagem in mensagens)
        {
            try
            {
                await _publicador.PublicarAsync(mensagem.ParaEnvelope(), cancellationToken);
                mensagem.MarcarComoProcessado(_relogio.Agora);
                publicadas++;
            }
            catch (Exception exception)
            {
                mensagem.RegistrarFalha(exception.Message);
                _logger.LogWarning(
                    exception,
                    "Falha ao publicar mensagem Outbox {MensagemId} com routing key {RoutingKey}",
                    mensagem.Id,
                    mensagem.RoutingKey);
            }
        }

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return publicadas;
    }
}
