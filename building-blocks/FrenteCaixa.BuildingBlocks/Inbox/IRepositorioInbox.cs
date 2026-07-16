namespace FrenteCaixa.BuildingBlocks.Inbox;

public interface IRepositorioInbox
{
    Task<bool> JaFoiProcessadaAsync(Guid mensagemId, CancellationToken cancellationToken);

    Task RegistrarProcessamentoAsync(RegistroInbox registro, CancellationToken cancellationToken);
}
