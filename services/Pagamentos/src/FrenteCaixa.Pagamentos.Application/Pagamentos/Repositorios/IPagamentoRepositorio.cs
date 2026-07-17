using FrenteCaixa.Pagamentos.Domain.Pagamentos;

namespace FrenteCaixa.Pagamentos.Application.Pagamentos.Repositorios;

public interface IPagamentoRepositorio
{
    Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Pagamento>> ListarPorVendaAsync(
        Guid vendaId,
        CancellationToken cancellationToken);

    Task AdicionarAsync(Pagamento pagamento, CancellationToken cancellationToken);
}
