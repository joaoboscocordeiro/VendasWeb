using FrenteCaixa.Estoque.Application.Estoques.Contratos;
using FrenteCaixa.Estoque.Application.Estoques.Interfaces;
using FrenteCaixa.Estoque.Application.Estoques.Repositorios;
using FrenteCaixa.Estoque.Domain.Estoques;

namespace FrenteCaixa.Estoque.Application.Estoques;

public sealed class ServicoEstoque : IServicoEstoque
{
    private readonly IEstoqueRepositorio _estoque;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ServicoEstoque(
        IEstoqueRepositorio estoque,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _estoque = estoque;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
    }

    public async Task<SaldoProdutoResponse> ObterSaldoAsync(Guid produtoId, CancellationToken cancellationToken)
    {
        var saldo = await _estoque.ObterSaldoPorProdutoAsync(produtoId, cancellationToken);

        return saldo is null
            ? new SaldoProdutoResponse(produtoId, 0, DateTimeOffset.UnixEpoch)
            : MapearSaldo(saldo);
    }

    public async Task<IReadOnlyCollection<MovimentacaoEstoqueResponse>> ListarMovimentacoesAsync(
        Guid produtoId,
        CancellationToken cancellationToken)
    {
        var movimentacoes = await _estoque.ListarMovimentacoesPorProdutoAsync(produtoId, cancellationToken);

        return movimentacoes.Select(MapearMovimentacao).ToArray();
    }

    public async Task<ResultadoOperacao<MovimentacaoEstoqueResponse>> RegistrarAjusteAsync(
        RegistrarAjusteEstoqueRequest request,
        CancellationToken cancellationToken)
    {
        var validacao = ValidarRequest(request, out var tipo);

        if (validacao is not null)
        {
            return ResultadoOperacao<MovimentacaoEstoqueResponse>.FalhaValidacao(validacao);
        }

        var saldo = await _estoque.ObterSaldoPorProdutoAsync(request.ProdutoId, cancellationToken);

        if (saldo is null)
        {
            saldo = SaldoProduto.Criar(request.ProdutoId, _relogio.Agora);
            await _estoque.AdicionarSaldoAsync(saldo, cancellationToken);
        }

        if (tipo == TipoMovimentacaoEstoque.Saida && saldo.QuantidadeDisponivel < request.Quantidade)
        {
            return ResultadoOperacao<MovimentacaoEstoqueResponse>.Conflito("Ajuste de saida deixaria o saldo negativo.");
        }

        var movimentacao = saldo.AplicarAjuste(
            tipo,
            request.Quantidade,
            request.Motivo.Trim(),
            _relogio.Agora);

        await _estoque.AdicionarMovimentacaoAsync(movimentacao, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<MovimentacaoEstoqueResponse>.Ok(MapearMovimentacao(movimentacao));
    }

    public async Task<ResultadoOperacao<IReadOnlyCollection<MovimentacaoEstoqueResponse>>> RegistrarDeducaoVendaAsync(
        DeducaoEstoqueRequest request,
        CancellationToken cancellationToken)
    {
        var validacao = ValidarDeducao(request);

        if (validacao is not null)
        {
            return ResultadoOperacao<IReadOnlyCollection<MovimentacaoEstoqueResponse>>.FalhaValidacao(validacao);
        }

        var itensAgrupados = request.Itens
            .GroupBy(item => item.ProdutoId)
            .Select(grupo => new ItemDeducaoEstoqueRequest(grupo.Key, grupo.Sum(item => item.Quantidade)))
            .ToArray();
        var saldos = new Dictionary<Guid, SaldoProduto>();

        foreach (var item in itensAgrupados)
        {
            var saldo = await _estoque.ObterSaldoPorProdutoAsync(item.ProdutoId, cancellationToken);

            if (saldo is null || saldo.QuantidadeDisponivel < item.Quantidade)
            {
                return ResultadoOperacao<IReadOnlyCollection<MovimentacaoEstoqueResponse>>.Conflito(
                    $"Estoque insuficiente para o produto {item.ProdutoId}.");
            }

            saldos[item.ProdutoId] = saldo;
        }

        var movimentacoes = new List<MovimentacaoEstoque>();
        var motivo = $"Deducao da venda {request.VendaId}";

        foreach (var item in itensAgrupados)
        {
            var movimentacao = saldos[item.ProdutoId].AplicarAjuste(
                TipoMovimentacaoEstoque.Saida,
                item.Quantidade,
                motivo,
                _relogio.Agora);

            await _estoque.AdicionarMovimentacaoAsync(movimentacao, cancellationToken);
            movimentacoes.Add(movimentacao);
        }

        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<IReadOnlyCollection<MovimentacaoEstoqueResponse>>.Ok(
            movimentacoes.Select(MapearMovimentacao).ToArray());
    }

    private static string? ValidarRequest(
        RegistrarAjusteEstoqueRequest request,
        out TipoMovimentacaoEstoque tipo)
    {
        tipo = default;

        if (request.ProdutoId == Guid.Empty)
        {
            return "ProdutoId e obrigatorio.";
        }

        if (!Enum.TryParse(request.Tipo, ignoreCase: true, out tipo)
            || !Enum.IsDefined(typeof(TipoMovimentacaoEstoque), tipo))
        {
            return "Tipo de ajuste deve ser Entrada ou Saida.";
        }

        if (request.Quantidade <= 0)
        {
            return "Quantidade deve ser maior que zero.";
        }

        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            return "Motivo e obrigatorio.";
        }

        if (request.Motivo.Trim().Length > 300)
        {
            return "Motivo deve possuir no maximo 300 caracteres.";
        }

        return null;
    }

    private static string? ValidarDeducao(DeducaoEstoqueRequest request)
    {
        if (request.VendaId == Guid.Empty)
        {
            return "VendaId e obrigatorio.";
        }

        if (request.Itens.Count == 0)
        {
            return "Ao menos um item e obrigatorio.";
        }

        if (request.Itens.Any(item => item.ProdutoId == Guid.Empty))
        {
            return "ProdutoId e obrigatorio.";
        }

        if (request.Itens.Any(item => item.Quantidade <= 0))
        {
            return "Quantidade deve ser maior que zero.";
        }

        return null;
    }

    private static SaldoProdutoResponse MapearSaldo(SaldoProduto saldo)
    {
        return new SaldoProdutoResponse(
            saldo.ProdutoId,
            saldo.QuantidadeDisponivel,
            saldo.AtualizadoEm);
    }

    private static MovimentacaoEstoqueResponse MapearMovimentacao(MovimentacaoEstoque movimentacao)
    {
        return new MovimentacaoEstoqueResponse(
            movimentacao.Id,
            movimentacao.ProdutoId,
            movimentacao.Tipo.ToString(),
            movimentacao.Quantidade,
            movimentacao.QuantidadeAnterior,
            movimentacao.QuantidadeAtual,
            movimentacao.Motivo,
            movimentacao.CriadaEm);
    }
}
