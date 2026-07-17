using FrenteCaixa.Pagamentos.Application.Pagamentos.Repositorios;
using FrenteCaixa.Pagamentos.Domain.Pagamentos;

namespace FrenteCaixa.Pagamentos.Tests;

public sealed class PagamentoRepositorioEmMemoria : IPagamentoRepositorio
{
    private readonly BancoPagamentosEmMemoria _banco;

    public PagamentoRepositorioEmMemoria(BancoPagamentosEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pagamento = _banco.Pagamentos.SingleOrDefault(item => item.Id == id);

        return Task.FromResult(pagamento);
    }

    public Task<IReadOnlyCollection<Pagamento>> ListarPorVendaAsync(
        Guid vendaId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Pagamento> pagamentos = _banco.Pagamentos
            .Where(item => item.VendaId == vendaId)
            .OrderBy(item => item.CriadoEm)
            .ToArray();

        return Task.FromResult(pagamentos);
    }

    public Task AdicionarAsync(Pagamento pagamento, CancellationToken cancellationToken)
    {
        _banco.Pagamentos.Add(pagamento);

        return Task.CompletedTask;
    }
}
