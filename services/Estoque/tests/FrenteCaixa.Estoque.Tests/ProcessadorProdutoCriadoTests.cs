using FrenteCaixa.Estoque.Application.Estoques.Eventos;
using FrenteCaixa.Estoque.Domain.Estoques;

namespace FrenteCaixa.Estoque.Tests;

public sealed class ProcessadorProdutoCriadoTests
{
    [Fact]
    public async Task stock_product_created_consumer_creates_zero_balance()
    {
        var banco = new BancoEstoqueEmMemoria();
        var processador = CriarProcessador(banco);
        var produtoId = Guid.NewGuid();

        await processador.ProcessarAsync(
            Guid.NewGuid(),
            "catalogo.produto-criado.v1",
            new ProdutoCriadoEvento(produtoId, "Cafe Torrado", "7891234567895", DateTimeOffset.UtcNow),
            CancellationToken.None);

        var saldo = Assert.Single(banco.Saldos);
        Assert.Equal(produtoId, saldo.ProdutoId);
        Assert.Equal(0, saldo.QuantidadeDisponivel);
    }

    [Fact]
    public async Task stock_product_created_consumer_ignores_duplicate_message()
    {
        var banco = new BancoEstoqueEmMemoria();
        var processador = CriarProcessador(banco);
        var mensagemId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var evento = new ProdutoCriadoEvento(produtoId, "Leite Integral", "7891234567896", DateTimeOffset.UtcNow);

        await processador.ProcessarAsync(mensagemId, "catalogo.produto-criado.v1", evento, CancellationToken.None);
        await processador.ProcessarAsync(mensagemId, "catalogo.produto-criado.v1", evento, CancellationToken.None);

        Assert.Single(banco.Saldos);
        Assert.Single(banco.MensagensProcessadas);
    }

    [Fact]
    public async Task stock_product_created_consumer_preserves_existing_balance()
    {
        var banco = new BancoEstoqueEmMemoria();
        var processador = CriarProcessador(banco);
        var produtoId = Guid.NewGuid();
        var saldoExistente = SaldoProduto.Criar(produtoId, DateTimeOffset.UtcNow);
        saldoExistente.AplicarAjuste(TipoMovimentacaoEstoque.Entrada, 12, "Saldo existente", DateTimeOffset.UtcNow);
        banco.Saldos.Add(saldoExistente);

        await processador.ProcessarAsync(
            Guid.NewGuid(),
            "catalogo.produto-criado.v1",
            new ProdutoCriadoEvento(produtoId, "Arroz Tipo 1", "7891234567897", DateTimeOffset.UtcNow),
            CancellationToken.None);

        var saldo = Assert.Single(banco.Saldos);
        Assert.Equal(12, saldo.QuantidadeDisponivel);
    }

    private static ProcessadorProdutoCriado CriarProcessador(BancoEstoqueEmMemoria banco)
    {
        return new ProcessadorProdutoCriado(
            new EstoqueRepositorioEmMemoria(banco),
            new InboxRepositorioEstoqueEmMemoria(banco),
            new RelogioFixo(),
            new UnidadeTrabalhoEmMemoria());
    }
}
