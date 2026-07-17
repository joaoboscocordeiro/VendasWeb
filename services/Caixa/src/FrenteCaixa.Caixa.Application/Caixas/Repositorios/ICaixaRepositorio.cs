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

    Task<IReadOnlyCollection<MovimentacaoCaixa>> ListarMovimentacoesAsync(
        Guid caixaId,
        CancellationToken cancellationToken);
}
