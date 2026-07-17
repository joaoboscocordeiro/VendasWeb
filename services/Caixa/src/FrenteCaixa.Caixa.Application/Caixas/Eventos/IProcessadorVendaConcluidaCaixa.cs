namespace FrenteCaixa.Caixa.Application.Caixas.Eventos;

public interface IProcessadorVendaConcluidaCaixa
{
    Task ProcessarAsync(
        Guid mensagemId,
        string routingKey,
        VendaConcluidaEvento evento,
        CancellationToken cancellationToken);
}
