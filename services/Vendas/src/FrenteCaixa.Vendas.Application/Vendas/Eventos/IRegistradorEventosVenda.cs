using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Application.Vendas.Eventos;

public interface IRegistradorEventosVenda
{
    Task RegistrarVendaConcluidaAsync(
        Venda venda,
        Guid pagamentoId,
        string formaPagamento,
        CancellationToken cancellationToken);
}
