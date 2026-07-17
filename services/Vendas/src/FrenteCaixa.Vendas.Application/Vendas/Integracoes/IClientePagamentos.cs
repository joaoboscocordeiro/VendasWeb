namespace FrenteCaixa.Vendas.Application.Vendas.Integracoes;

public interface IClientePagamentos
{
    Task<ResultadoIntegracao<PagamentoRegistradoIntegracao>> RegistrarPagamentoAsync(
        Guid vendaId,
        string formaPagamento,
        decimal valorVenda,
        decimal valorPago,
        string accessToken,
        CancellationToken cancellationToken);
}
