using FrenteCaixa.Caixa.Domain.Caixas;

namespace FrenteCaixa.Caixa.Application.Caixas.Repositorios;

public interface ICaixaRepositorio
{
    Task<CaixaOperacional?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CaixaOperacional?> ObterCaixaAbertoPorOperadorAsync(
        Guid operadorId,
        CancellationToken cancellationToken);

    Task AdicionarCaixaAsync(CaixaOperacional caixa, CancellationToken cancellationToken);

    Task AdicionarMovimentacaoAsync(MovimentacaoCaixa movimentacao, CancellationToken cancellationToken);

    Task<VendaCaixaProjetada?> ObterVendaProjetadaAsync(
        Guid vendaId,
        CancellationToken cancellationToken);

    Task AdicionarVendaProjetadaAsync(VendaCaixaProjetada venda, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MovimentacaoCaixa>> ListarMovimentacoesAsync(
        Guid caixaId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<VendaCaixaProjetada>> ListarVendasProjetadasAsync(
        Guid caixaId,
        CancellationToken cancellationToken);
}
