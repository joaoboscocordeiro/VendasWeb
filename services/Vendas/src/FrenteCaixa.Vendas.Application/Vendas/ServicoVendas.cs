using FrenteCaixa.Vendas.Application.Vendas.Contratos;
using FrenteCaixa.Vendas.Application.Vendas.Eventos;
using FrenteCaixa.Vendas.Application.Vendas.Interfaces;
using FrenteCaixa.Vendas.Application.Vendas.Integracoes;
using FrenteCaixa.Vendas.Application.Vendas.Repositorios;
using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Application.Vendas;

public sealed class ServicoVendas : IServicoVendas
{
    private readonly IVendaRepositorio _vendas;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;
    private readonly IClienteCaixa _clienteCaixa;
    private readonly IClienteEstoque _clienteEstoque;
    private readonly IClientePagamentos _clientePagamentos;
    private readonly IRegistradorEventosVenda _registradorEventos;

    public ServicoVendas(
        IVendaRepositorio vendas,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho,
        IClienteCaixa clienteCaixa,
        IClienteEstoque clienteEstoque,
        IClientePagamentos clientePagamentos,
        IRegistradorEventosVenda registradorEventos)
    {
        _vendas = vendas;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
        _clienteCaixa = clienteCaixa;
        _clienteEstoque = clienteEstoque;
        _clientePagamentos = clientePagamentos;
        _registradorEventos = registradorEventos;
    }

    public async Task<ResultadoOperacao<VendaResponse>> IniciarAsync(
        Guid operadorId,
        IniciarVendaRequest request,
        CancellationToken cancellationToken)
    {
        if (operadorId == Guid.Empty)
        {
            return ResultadoOperacao<VendaResponse>.FalhaValidacao("Operador autenticado e obrigatorio.");
        }

        if (request.CaixaId == Guid.Empty)
        {
            return ResultadoOperacao<VendaResponse>.FalhaValidacao("CaixaId e obrigatorio.");
        }

        var venda = Venda.Iniciar(operadorId, request.CaixaId, _relogio.Agora);

        await _vendas.AdicionarAsync(venda, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<VendaResponse>.Ok(MapearVenda(venda));
    }

    public async Task<ResultadoOperacao<CheckoutVendaResponse>> FinalizarAsync(
        Guid operadorId,
        Guid vendaId,
        FinalizarVendaRequest request,
        string accessToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return ResultadoOperacao<CheckoutVendaResponse>.FalhaValidacao("Access token e obrigatorio.");
        }

        var validacaoPagamento = ValidarPagamento(request);

        if (validacaoPagamento is not null)
        {
            return ResultadoOperacao<CheckoutVendaResponse>.FalhaValidacao(validacaoPagamento);
        }

        var venda = await ObterVendaDoOperadorAsync(operadorId, vendaId, cancellationToken);

        if (venda is null)
        {
            return ResultadoOperacao<CheckoutVendaResponse>.NaoEncontrado("Venda nao encontrada.");
        }

        if (venda.Status != StatusVenda.EmAndamento)
        {
            return ResultadoOperacao<CheckoutVendaResponse>.Conflito("Venda nao esta em andamento.");
        }

        if (venda.Itens.Count == 0)
        {
            return ResultadoOperacao<CheckoutVendaResponse>.FalhaValidacao("Venda deve possuir ao menos um item.");
        }

        var caixa = await _clienteCaixa.ObterCaixaAtualAsync(accessToken, cancellationToken);

        if (!caixa.Sucesso)
        {
            return MapearFalhaIntegracao<CheckoutVendaResponse, CaixaAtualIntegracao>(
                caixa,
                "Nao foi possivel validar o caixa aberto.");
        }

        if (caixa.Valor!.Id != venda.CaixaId
            || caixa.Valor.OperadorId != operadorId
            || !string.Equals(caixa.Valor.Status, "Aberto", StringComparison.OrdinalIgnoreCase))
        {
            return ResultadoOperacao<CheckoutVendaResponse>.Conflito("Caixa da venda nao esta aberto para o operador.");
        }

        var deducaoEstoque = await _clienteEstoque.DeduzirVendaAsync(
            venda.Id,
            venda.Itens.ToArray(),
            accessToken,
            cancellationToken);

        if (!deducaoEstoque.Sucesso)
        {
            return MapearFalhaIntegracao<CheckoutVendaResponse, IReadOnlyCollection<Guid>>(
                deducaoEstoque,
                "Nao foi possivel deduzir estoque.");
        }

        var pagamento = await _clientePagamentos.RegistrarPagamentoAsync(
            venda.Id,
            request.FormaPagamento.Trim(),
            venda.Total,
            request.ValorPago,
            accessToken,
            cancellationToken);

        if (!pagamento.Sucesso)
        {
            return MapearFalhaIntegracao<CheckoutVendaResponse, PagamentoRegistradoIntegracao>(
                pagamento,
                "Nao foi possivel registrar pagamento.");
        }

        venda.Concluir(_relogio.Agora);
        await _registradorEventos.RegistrarVendaConcluidaAsync(
            venda,
            pagamento.Valor!.Id,
            pagamento.Valor.FormaPagamento,
            cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<CheckoutVendaResponse>.Ok(
            new CheckoutVendaResponse(MapearVenda(venda), pagamento.Valor.Id, venda.AtualizadaEm));
    }

    public async Task<ResultadoOperacao<VendaResponse>> ObterAsync(
        Guid operadorId,
        Guid vendaId,
        CancellationToken cancellationToken)
    {
        var venda = await ObterVendaDoOperadorAsync(operadorId, vendaId, cancellationToken);

        return venda is null
            ? ResultadoOperacao<VendaResponse>.NaoEncontrado("Venda nao encontrada.")
            : ResultadoOperacao<VendaResponse>.Ok(MapearVenda(venda));
    }

    public async Task<ResultadoOperacao<VendaResponse>> AdicionarItemAsync(
        Guid operadorId,
        Guid vendaId,
        AdicionarItemVendaRequest request,
        CancellationToken cancellationToken)
    {
        var validacao = ValidarItem(request);

        if (validacao is not null)
        {
            return ResultadoOperacao<VendaResponse>.FalhaValidacao(validacao);
        }

        var venda = await ObterVendaDoOperadorAsync(operadorId, vendaId, cancellationToken);

        if (venda is null)
        {
            return ResultadoOperacao<VendaResponse>.NaoEncontrado("Venda nao encontrada.");
        }

        if (venda.Status != StatusVenda.EmAndamento)
        {
            return ResultadoOperacao<VendaResponse>.Conflito("Venda nao esta em andamento.");
        }

        venda.AdicionarItem(
            request.ProdutoId,
            request.DescricaoProduto,
            request.Quantidade,
            request.PrecoUnitario,
            _relogio.Agora);

        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<VendaResponse>.Ok(MapearVenda(venda));
    }

    public async Task<ResultadoOperacao<VendaResponse>> RemoverItemAsync(
        Guid operadorId,
        Guid vendaId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var venda = await ObterVendaDoOperadorAsync(operadorId, vendaId, cancellationToken);

        if (venda is null)
        {
            return ResultadoOperacao<VendaResponse>.NaoEncontrado("Venda nao encontrada.");
        }

        if (!venda.RemoverItem(itemId, _relogio.Agora))
        {
            return ResultadoOperacao<VendaResponse>.NaoEncontrado("Item da venda nao encontrado.");
        }

        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<VendaResponse>.Ok(MapearVenda(venda));
    }

    private async Task<Venda?> ObterVendaDoOperadorAsync(
        Guid operadorId,
        Guid vendaId,
        CancellationToken cancellationToken)
    {
        var venda = await _vendas.ObterPorIdAsync(vendaId, cancellationToken);

        return venda is null || venda.OperadorId != operadorId ? null : venda;
    }

    private static string? ValidarItem(AdicionarItemVendaRequest request)
    {
        if (request.ProdutoId == Guid.Empty)
        {
            return "ProdutoId e obrigatorio.";
        }

        if (string.IsNullOrWhiteSpace(request.DescricaoProduto))
        {
            return "Descricao do produto e obrigatoria.";
        }

        if (request.DescricaoProduto.Trim().Length > 220)
        {
            return "Descricao do produto deve possuir no maximo 220 caracteres.";
        }

        if (request.Quantidade <= 0)
        {
            return "Quantidade deve ser maior que zero.";
        }

        if (request.PrecoUnitario <= 0)
        {
            return "Preco unitario deve ser maior que zero.";
        }

        return null;
    }

    private static string? ValidarPagamento(FinalizarVendaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FormaPagamento))
        {
            return "Forma de pagamento e obrigatoria.";
        }

        if (request.ValorPago <= 0)
        {
            return "Valor pago deve ser maior que zero.";
        }

        return null;
    }

    private static ResultadoOperacao<T> MapearFalhaIntegracao<T, TIntegracao>(
        ResultadoIntegracao<TIntegracao> resultado,
        string mensagemPadrao)
    {
        var erro = resultado.Erro ?? mensagemPadrao;

        return resultado.CodigoErro switch
        {
            CodigoErroIntegracao.Validacao => ResultadoOperacao<T>.FalhaValidacao(erro),
            CodigoErroIntegracao.Conflito => ResultadoOperacao<T>.Conflito(erro),
            CodigoErroIntegracao.NaoEncontrado => ResultadoOperacao<T>.NaoEncontrado(erro),
            _ => ResultadoOperacao<T>.DependenciaIndisponivel(erro)
        };
    }

    private static VendaResponse MapearVenda(Venda venda)
    {
        var itens = venda.Itens
            .OrderBy(item => item.CriadoEm)
            .Select(MapearItem)
            .ToArray();

        return new VendaResponse(
            venda.Id,
            venda.OperadorId,
            venda.CaixaId,
            venda.Status.ToString(),
            venda.Total,
            itens,
            venda.CriadaEm,
            venda.AtualizadaEm);
    }

    private static ItemVendaResponse MapearItem(ItemVenda item)
    {
        return new ItemVendaResponse(
            item.Id,
            item.ProdutoId,
            item.DescricaoProduto,
            item.Quantidade,
            item.PrecoUnitario,
            item.Subtotal,
            item.CriadoEm);
    }
}
