namespace FrenteCaixa.Bff.Api.Admin;

public interface IAdminRelatoriosService
{
    Task<IReadOnlyCollection<AdminVendaRelatorioResponse>> ListarVendasAsync(
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<AdminResumoFinanceiroResponse> ObterResumoFinanceiroAsync(
        string authorizationHeader,
        CancellationToken cancellationToken);
}
