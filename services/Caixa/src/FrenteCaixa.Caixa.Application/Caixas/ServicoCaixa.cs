using FrenteCaixa.Caixa.Application.Caixas.Contratos;
using FrenteCaixa.Caixa.Application.Caixas.Interfaces;
using FrenteCaixa.Caixa.Application.Caixas.Repositorios;
using FrenteCaixa.Caixa.Domain.Caixas;

namespace FrenteCaixa.Caixa.Application.Caixas;

public sealed class ServicoCaixa : IServicoCaixa
{
    private readonly ICaixaRepositorio _caixas;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ServicoCaixa(
        ICaixaRepositorio caixas,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _caixas = caixas;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
    }

    public async Task<ResultadoOperacao<CaixaOperacionalResponse>> AbrirAsync(
        Guid operadorId,
        AbrirCaixaRequest request,
        CancellationToken cancellationToken)
    {
        if (operadorId == Guid.Empty)
        {
            return ResultadoOperacao<CaixaOperacionalResponse>.FalhaValidacao("Operador autenticado e obrigatorio.");
        }

        if (request.ValorInicial < 0)
        {
            return ResultadoOperacao<CaixaOperacionalResponse>.FalhaValidacao("Valor inicial nao pode ser negativo.");
        }

        var caixaAberto = await _caixas.ObterCaixaAbertoPorOperadorAsync(operadorId, cancellationToken);

        if (caixaAberto is not null)
        {
            return ResultadoOperacao<CaixaOperacionalResponse>.Conflito("Operador ja possui caixa aberto.");
        }

        var caixa = CaixaOperacional.Abrir(operadorId, request.ValorInicial, _relogio.Agora);
        var movimentacao = caixa.RegistrarAbertura(_relogio.Agora);

        await _caixas.AdicionarCaixaAsync(caixa, cancellationToken);
        await _caixas.AdicionarMovimentacaoAsync(movimentacao, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<CaixaOperacionalResponse>.Ok(MapearCaixa(caixa));
    }

    public async Task<ResultadoOperacao<CaixaOperacionalResponse>> ObterAtualAsync(
        Guid operadorId,
        CancellationToken cancellationToken)
    {
        var caixa = await _caixas.ObterCaixaAbertoPorOperadorAsync(operadorId, cancellationToken);

        return caixa is null
            ? ResultadoOperacao<CaixaOperacionalResponse>.NaoEncontrado("Caixa aberto nao encontrado.")
            : ResultadoOperacao<CaixaOperacionalResponse>.Ok(MapearCaixa(caixa));
    }

    public async Task<ResultadoOperacao<CaixaOperacionalResponse>> FecharAsync(
        Guid operadorId,
        Guid caixaId,
        FecharCaixaRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ValorFechamento < 0)
        {
            return ResultadoOperacao<CaixaOperacionalResponse>.FalhaValidacao("Valor de fechamento nao pode ser negativo.");
        }

        var caixa = await _caixas.ObterPorIdAsync(caixaId, cancellationToken);

        if (caixa is null || caixa.OperadorId != operadorId)
        {
            return ResultadoOperacao<CaixaOperacionalResponse>.NaoEncontrado("Caixa nao encontrado.");
        }

        if (caixa.Status == StatusCaixa.Fechado)
        {
            return ResultadoOperacao<CaixaOperacionalResponse>.Conflito("Caixa ja esta fechado.");
        }

        var movimentacao = caixa.Fechar(request.ValorFechamento, _relogio.Agora);

        await _caixas.AdicionarMovimentacaoAsync(movimentacao, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<CaixaOperacionalResponse>.Ok(MapearCaixa(caixa));
    }

    public async Task<ResultadoOperacao<IReadOnlyCollection<MovimentacaoCaixaResponse>>> ListarMovimentacoesAsync(
        Guid operadorId,
        Guid caixaId,
        CancellationToken cancellationToken)
    {
        var caixa = await _caixas.ObterPorIdAsync(caixaId, cancellationToken);

        if (caixa is null || caixa.OperadorId != operadorId)
        {
            return ResultadoOperacao<IReadOnlyCollection<MovimentacaoCaixaResponse>>.NaoEncontrado("Caixa nao encontrado.");
        }

        var movimentacoes = await _caixas.ListarMovimentacoesAsync(caixaId, cancellationToken);
        var response = movimentacoes.Select(MapearMovimentacao).ToArray();

        return ResultadoOperacao<IReadOnlyCollection<MovimentacaoCaixaResponse>>.Ok(response);
    }

    public async Task<ResultadoOperacao<ResumoCaixaResponse>> ObterResumoAsync(
        Guid operadorId,
        Guid caixaId,
        CancellationToken cancellationToken)
    {
        var caixa = await _caixas.ObterPorIdAsync(caixaId, cancellationToken);

        if (caixa is null || caixa.OperadorId != operadorId)
        {
            return ResultadoOperacao<ResumoCaixaResponse>.NaoEncontrado("Caixa nao encontrado.");
        }

        var vendas = await _caixas.ListarVendasProjetadasAsync(caixaId, cancellationToken);
        var totaisPorFormaPagamento = vendas
            .GroupBy(venda => venda.FormaPagamento)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new TotalFormaPagamentoCaixaResponse(
                grupo.Key,
                grupo.Count(),
                grupo.Sum(venda => venda.ValorTotal)))
            .ToArray();

        var totalVendido = vendas.Sum(venda => venda.ValorTotal);
        var dinheiroVendido = totaisPorFormaPagamento
            .Where(total => string.Equals(total.FormaPagamento, "Dinheiro", StringComparison.OrdinalIgnoreCase))
            .Sum(total => total.Total);
        var dinheiroEsperado = caixa.ValorInicial + dinheiroVendido;
        var diferencaPrevista = caixa.ValorFechamento is null
            ? (decimal?)null
            : caixa.ValorFechamento.Value - dinheiroEsperado;

        return ResultadoOperacao<ResumoCaixaResponse>.Ok(new ResumoCaixaResponse(
            caixa.Id,
            caixa.OperadorId,
            caixa.Status.ToString(),
            caixa.ValorInicial,
            caixa.ValorFechamento,
            vendas.Count,
            totalVendido,
            dinheiroEsperado,
            diferencaPrevista,
            totaisPorFormaPagamento));
    }

    private static CaixaOperacionalResponse MapearCaixa(CaixaOperacional caixa)
    {
        return new CaixaOperacionalResponse(
            caixa.Id,
            caixa.OperadorId,
            caixa.ValorInicial,
            caixa.ValorFechamento,
            caixa.Status.ToString(),
            caixa.AbertoEm,
            caixa.FechadoEm,
            caixa.AtualizadoEm);
    }

    private static MovimentacaoCaixaResponse MapearMovimentacao(MovimentacaoCaixa movimentacao)
    {
        return new MovimentacaoCaixaResponse(
            movimentacao.Id,
            movimentacao.CaixaId,
            movimentacao.OperadorId,
            movimentacao.Tipo.ToString(),
            movimentacao.Valor,
            movimentacao.Descricao,
            movimentacao.CriadaEm);
    }
}
