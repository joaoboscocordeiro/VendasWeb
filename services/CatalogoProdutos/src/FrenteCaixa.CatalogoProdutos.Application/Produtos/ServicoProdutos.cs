using FrenteCaixa.CatalogoProdutos.Application.Produtos.Contratos;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Repositorios;
using FrenteCaixa.CatalogoProdutos.Domain.Produtos;

namespace FrenteCaixa.CatalogoProdutos.Application.Produtos;

public sealed class ServicoProdutos : IServicoProdutos
{
    private readonly IProdutoRepositorio _produtos;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ServicoProdutos(
        IProdutoRepositorio produtos,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _produtos = produtos;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
    }

    public async Task<ResultadoOperacao<ProdutoResponse>> CadastrarAsync(
        CadastrarProdutoRequest request,
        CancellationToken cancellationToken)
    {
        var validacao = ValidarDados(request.Descricao, request.CodigoBarrasEan, request.PrecoCusto, request.PrecoVenda);

        if (validacao is not null)
        {
            return ResultadoOperacao<ProdutoResponse>.FalhaValidacao(validacao);
        }

        var codigoBarras = NormalizarCodigoBarras(request.CodigoBarrasEan);

        if (codigoBarras is not null && await _produtos.ObterPorCodigoBarrasAsync(codigoBarras, cancellationToken) is not null)
        {
            return ResultadoOperacao<ProdutoResponse>.Conflito("Ja existe produto com este codigo de barras.");
        }

        var produto = Produto.Criar(
            request.Descricao,
            codigoBarras,
            request.PrecoCusto,
            request.PrecoVenda,
            _relogio.Agora);

        await _produtos.AdicionarAsync(produto, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<ProdutoResponse>.Ok(Mapear(produto));
    }

    public async Task<IReadOnlyCollection<ProdutoResponse>> ListarAsync(CancellationToken cancellationToken)
    {
        var produtos = await _produtos.ListarAsync(cancellationToken);

        return produtos.Select(Mapear).ToArray();
    }

    public async Task<ResultadoOperacao<ProdutoResponse>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var produto = await _produtos.ObterPorIdAsync(id, cancellationToken);

        return produto is null
            ? ResultadoOperacao<ProdutoResponse>.NaoEncontrado("Produto nao encontrado.")
            : ResultadoOperacao<ProdutoResponse>.Ok(Mapear(produto));
    }

    public async Task<ResultadoOperacao<ProdutoResponse>> ObterPorCodigoBarrasAsync(
        string codigoBarrasEan,
        CancellationToken cancellationToken)
    {
        var codigoBarras = NormalizarCodigoBarras(codigoBarrasEan);

        if (codigoBarras is null)
        {
            return ResultadoOperacao<ProdutoResponse>.FalhaValidacao("Codigo de barras e obrigatorio.");
        }

        var produto = await _produtos.ObterPorCodigoBarrasAsync(codigoBarras, cancellationToken);

        return produto is null
            ? ResultadoOperacao<ProdutoResponse>.NaoEncontrado("Produto nao encontrado.")
            : ResultadoOperacao<ProdutoResponse>.Ok(Mapear(produto));
    }

    public async Task<ResultadoOperacao<ProdutoResponse>> AtualizarAsync(
        Guid id,
        AtualizarProdutoRequest request,
        CancellationToken cancellationToken)
    {
        var validacao = ValidarDados(request.Descricao, request.CodigoBarrasEan, request.PrecoCusto, request.PrecoVenda);

        if (validacao is not null)
        {
            return ResultadoOperacao<ProdutoResponse>.FalhaValidacao(validacao);
        }

        var produto = await _produtos.ObterPorIdAsync(id, cancellationToken);

        if (produto is null)
        {
            return ResultadoOperacao<ProdutoResponse>.NaoEncontrado("Produto nao encontrado.");
        }

        var codigoBarras = NormalizarCodigoBarras(request.CodigoBarrasEan);
        var produtoComCodigo = codigoBarras is null
            ? null
            : await _produtos.ObterPorCodigoBarrasAsync(codigoBarras, cancellationToken);

        if (produtoComCodigo is not null && produtoComCodigo.Id != id)
        {
            return ResultadoOperacao<ProdutoResponse>.Conflito("Ja existe produto com este codigo de barras.");
        }

        produto.Atualizar(
            request.Descricao,
            codigoBarras,
            request.PrecoCusto,
            request.PrecoVenda,
            _relogio.Agora);

        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<ProdutoResponse>.Ok(Mapear(produto));
    }

    public async Task<ResultadoOperacao<bool>> InativarAsync(Guid id, CancellationToken cancellationToken)
    {
        var produto = await _produtos.ObterPorIdAsync(id, cancellationToken);

        if (produto is null)
        {
            return ResultadoOperacao<bool>.NaoEncontrado("Produto nao encontrado.");
        }

        if (produto.Ativo)
        {
            produto.Inativar(_relogio.Agora);
            await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        }

        return ResultadoOperacao<bool>.Ok(true);
    }

    private static string? ValidarDados(
        string descricao,
        string? codigoBarrasEan,
        decimal precoCusto,
        decimal precoVenda)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            return "Descricao e obrigatoria.";
        }

        if (precoCusto < 0)
        {
            return "Preco de custo nao pode ser negativo.";
        }

        if (precoVenda <= 0)
        {
            return "Preco de venda deve ser maior que zero.";
        }

        var codigoBarras = NormalizarCodigoBarras(codigoBarrasEan);

        if (codigoBarras is not null && !codigoBarras.All(char.IsDigit))
        {
            return "Codigo de barras deve conter apenas numeros.";
        }

        return null;
    }

    private static ProdutoResponse Mapear(Produto produto)
    {
        return new ProdutoResponse(
            produto.Id,
            produto.Descricao,
            produto.CodigoBarrasEan,
            produto.PrecoCusto,
            produto.PrecoVenda,
            produto.Ativo,
            produto.CriadoEm,
            produto.AtualizadoEm);
    }

    private static string? NormalizarCodigoBarras(string? codigoBarrasEan)
    {
        return string.IsNullOrWhiteSpace(codigoBarrasEan)
            ? null
            : codigoBarrasEan.Trim();
    }
}
