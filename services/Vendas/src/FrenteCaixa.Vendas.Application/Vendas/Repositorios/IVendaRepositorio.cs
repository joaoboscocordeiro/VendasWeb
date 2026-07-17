using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Application.Vendas.Repositorios;

public interface IVendaRepositorio
{
    Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task AdicionarAsync(Venda venda, CancellationToken cancellationToken);
}
