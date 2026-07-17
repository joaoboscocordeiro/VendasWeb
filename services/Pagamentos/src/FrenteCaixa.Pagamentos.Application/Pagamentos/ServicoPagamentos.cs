using FrenteCaixa.Pagamentos.Application.Pagamentos.Contratos;
using FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;
using FrenteCaixa.Pagamentos.Application.Pagamentos.Repositorios;
using FrenteCaixa.Pagamentos.Domain.Pagamentos;

namespace FrenteCaixa.Pagamentos.Application.Pagamentos;

public sealed class ServicoPagamentos : IServicoPagamentos
{
    private readonly IPagamentoRepositorio _pagamentos;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ServicoPagamentos(
        IPagamentoRepositorio pagamentos,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _pagamentos = pagamentos;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
    }

    public async Task<ResultadoOperacao<PagamentoResponse>> RegistrarAsync(
        RegistrarPagamentoRequest request,
        CancellationToken cancellationToken)
    {
        var validacao = ValidarRequest(request, out var formaPagamento);

        if (validacao is not null)
        {
            return ResultadoOperacao<PagamentoResponse>.FalhaValidacao(validacao);
        }

        var pagamento = Pagamento.Registrar(
            request.VendaId,
            formaPagamento,
            request.ValorVenda,
            request.ValorPago,
            _relogio.Agora);

        await _pagamentos.AdicionarAsync(pagamento, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<PagamentoResponse>.Ok(MapearPagamento(pagamento));
    }

    public async Task<ResultadoOperacao<PagamentoResponse>> ObterAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var pagamento = await _pagamentos.ObterPorIdAsync(id, cancellationToken);

        return pagamento is null
            ? ResultadoOperacao<PagamentoResponse>.NaoEncontrado("Pagamento nao encontrado.")
            : ResultadoOperacao<PagamentoResponse>.Ok(MapearPagamento(pagamento));
    }

    public async Task<IReadOnlyCollection<PagamentoResponse>> ListarPorVendaAsync(
        Guid vendaId,
        CancellationToken cancellationToken)
    {
        var pagamentos = await _pagamentos.ListarPorVendaAsync(vendaId, cancellationToken);

        return pagamentos
            .OrderBy(pagamento => pagamento.CriadoEm)
            .Select(MapearPagamento)
            .ToArray();
    }

    private static string? ValidarRequest(
        RegistrarPagamentoRequest request,
        out FormaPagamento formaPagamento)
    {
        formaPagamento = default;

        if (request.VendaId == Guid.Empty)
        {
            return "VendaId e obrigatorio.";
        }

        if (!Enum.TryParse(request.FormaPagamento, ignoreCase: true, out formaPagamento)
            || !Enum.IsDefined(typeof(FormaPagamento), formaPagamento))
        {
            return "Forma de pagamento deve ser Dinheiro, Cartao ou Pix.";
        }

        if (request.ValorVenda <= 0)
        {
            return "Valor da venda deve ser maior que zero.";
        }

        if (request.ValorPago <= 0)
        {
            return "Valor pago deve ser maior que zero.";
        }

        if (formaPagamento == FormaPagamento.Dinheiro && request.ValorPago < request.ValorVenda)
        {
            return "Valor pago em dinheiro deve ser maior ou igual ao valor da venda.";
        }

        if (formaPagamento != FormaPagamento.Dinheiro && request.ValorPago != request.ValorVenda)
        {
            return "Valor pago em cartao ou Pix deve ser igual ao valor da venda.";
        }

        return null;
    }

    private static PagamentoResponse MapearPagamento(Pagamento pagamento)
    {
        return new PagamentoResponse(
            pagamento.Id,
            pagamento.VendaId,
            pagamento.FormaPagamento.ToString(),
            pagamento.ValorVenda,
            pagamento.ValorPago,
            pagamento.Troco,
            pagamento.Status.ToString(),
            pagamento.CriadoEm);
    }
}
