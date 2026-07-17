using FrenteCaixa.Vendas.Application.Vendas.Contratos;
using FrenteCaixa.Vendas.Application.Vendas.Interfaces;
using FrenteCaixa.Vendas.Application.Vendas.Repositorios;
using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Application.Vendas;

public sealed class ServicoVendas : IServicoVendas
{
    private readonly IVendaRepositorio _vendas;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ServicoVendas(
        IVendaRepositorio vendas,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _vendas = vendas;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
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
