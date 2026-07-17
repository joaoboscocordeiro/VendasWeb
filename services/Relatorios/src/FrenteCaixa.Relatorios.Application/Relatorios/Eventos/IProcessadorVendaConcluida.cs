namespace FrenteCaixa.Relatorios.Application.Relatorios.Eventos;

public interface IProcessadorVendaConcluida
{
    Task ProcessarAsync(
        Guid mensagemId,
        string routingKey,
        VendaConcluidaEvento evento,
        CancellationToken cancellationToken);
}
