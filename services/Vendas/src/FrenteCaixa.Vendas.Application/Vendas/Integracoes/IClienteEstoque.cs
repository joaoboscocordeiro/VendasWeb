using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Application.Vendas.Integracoes;

public interface IClienteEstoque
{
    Task<ResultadoIntegracao<IReadOnlyCollection<Guid>>> DeduzirVendaAsync(
        Guid vendaId,
        IReadOnlyCollection<ItemVenda> itens,
        string accessToken,
        CancellationToken cancellationToken);
}
