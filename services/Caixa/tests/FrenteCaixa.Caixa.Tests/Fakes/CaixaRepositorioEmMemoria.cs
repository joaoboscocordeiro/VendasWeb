using FrenteCaixa.Caixa.Application.Caixas.Repositorios;
using FrenteCaixa.Caixa.Domain.Caixas;

namespace FrenteCaixa.Caixa.Tests;

public sealed class CaixaRepositorioEmMemoria : ICaixaRepositorio
{
    private readonly BancoCaixaEmMemoria _banco;

    public CaixaRepositorioEmMemoria(BancoCaixaEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<CaixaOperacional?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var caixa = _banco.Caixas.SingleOrDefault(item => item.Id == id);

        return Task.FromResult(caixa);
    }

    public Task<CaixaOperacional?> ObterCaixaAbertoPorOperadorAsync(
        Guid operadorId,
        CancellationToken cancellationToken)
    {
        var caixa = _banco.Caixas.SingleOrDefault(
            item => item.OperadorId == operadorId && item.Status == StatusCaixa.Aberto);

        return Task.FromResult(caixa);
    }

    public Task AdicionarCaixaAsync(CaixaOperacional caixa, CancellationToken cancellationToken)
    {
        _banco.Caixas.Add(caixa);

        return Task.CompletedTask;
    }

    public Task AdicionarMovimentacaoAsync(MovimentacaoCaixa movimentacao, CancellationToken cancellationToken)
    {
        _banco.Movimentacoes.Add(movimentacao);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<MovimentacaoCaixa>> ListarMovimentacoesAsync(
        Guid caixaId,
        CancellationToken cancellationToken)
    {
        var movimentacoes = _banco.Movimentacoes
            .Where(movimentacao => movimentacao.CaixaId == caixaId)
            .OrderBy(movimentacao => movimentacao.CriadaEm)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<MovimentacaoCaixa>>(movimentacoes);
    }
}
