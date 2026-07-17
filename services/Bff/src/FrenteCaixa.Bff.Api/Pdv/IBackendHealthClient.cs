namespace FrenteCaixa.Bff.Api.Pdv;

public interface IBackendHealthClient
{
    Task<ServicoBackendStatusResponse> ObterStatusAsync(
        BackendServicoOptions servico,
        CancellationToken cancellationToken);
}
