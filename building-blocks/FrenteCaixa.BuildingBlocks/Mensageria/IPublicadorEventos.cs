namespace FrenteCaixa.BuildingBlocks.Mensageria;

public interface IPublicadorEventos
{
    Task PublicarAsync(EnvelopeEventoIntegracao evento, CancellationToken cancellationToken);
}
