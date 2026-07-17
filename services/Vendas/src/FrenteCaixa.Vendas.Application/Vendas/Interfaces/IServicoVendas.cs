using FrenteCaixa.Vendas.Application.Vendas.Contratos;

namespace FrenteCaixa.Vendas.Application.Vendas.Interfaces;

public interface IServicoVendas
{
    Task<ResultadoOperacao<VendaResponse>> IniciarAsync(
        Guid operadorId,
        IniciarVendaRequest request,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<VendaResponse>> ObterAsync(
        Guid operadorId,
        Guid vendaId,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<VendaResponse>> AdicionarItemAsync(
        Guid operadorId,
        Guid vendaId,
        AdicionarItemVendaRequest request,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<VendaResponse>> RemoverItemAsync(
        Guid operadorId,
        Guid vendaId,
        Guid itemId,
        CancellationToken cancellationToken);
}
