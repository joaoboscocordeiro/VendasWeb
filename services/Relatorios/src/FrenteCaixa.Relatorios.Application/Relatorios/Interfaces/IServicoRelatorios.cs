using FrenteCaixa.Relatorios.Application.Relatorios.Contratos;

namespace FrenteCaixa.Relatorios.Application.Relatorios.Interfaces;

public interface IServicoRelatorios
{
    Task<IReadOnlyCollection<VendaRelatorioResponse>> ListarVendasAsync(CancellationToken cancellationToken);

    Task<ResumoFinanceiroResponse> ObterResumoFinanceiroAsync(CancellationToken cancellationToken);
}
