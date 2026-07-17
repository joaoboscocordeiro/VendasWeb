using FrenteCaixa.Pagamentos.Application.Pagamentos.Contratos;

namespace FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;

public interface IServicoPagamentos
{
    Task<ResultadoOperacao<PagamentoResponse>> RegistrarAsync(
        RegistrarPagamentoRequest request,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<PagamentoResponse>> ObterAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PagamentoResponse>> ListarPorVendaAsync(
        Guid vendaId,
        CancellationToken cancellationToken);
}
