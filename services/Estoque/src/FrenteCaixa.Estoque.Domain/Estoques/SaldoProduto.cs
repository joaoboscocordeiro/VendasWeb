namespace FrenteCaixa.Estoque.Domain.Estoques;

public sealed class SaldoProduto
{
    private SaldoProduto()
    {
    }

    private SaldoProduto(Guid produtoId, DateTimeOffset atualizadoEm)
    {
        ProdutoId = produtoId;
        QuantidadeDisponivel = 0;
        AtualizadoEm = atualizadoEm;
    }

    public Guid ProdutoId { get; private set; }
    public decimal QuantidadeDisponivel { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }

    public static SaldoProduto Criar(Guid produtoId, DateTimeOffset criadoEm)
    {
        return new SaldoProduto(produtoId, criadoEm);
    }

    public MovimentacaoEstoque AplicarAjuste(
        TipoMovimentacaoEstoque tipo,
        decimal quantidade,
        string motivo,
        DateTimeOffset criadaEm)
    {
        var quantidadeAnterior = QuantidadeDisponivel;
        var quantidadeAtual = tipo == TipoMovimentacaoEstoque.Entrada
            ? QuantidadeDisponivel + quantidade
            : QuantidadeDisponivel - quantidade;

        QuantidadeDisponivel = quantidadeAtual;
        AtualizadoEm = criadaEm;

        return MovimentacaoEstoque.Criar(
            ProdutoId,
            tipo,
            quantidade,
            quantidadeAnterior,
            quantidadeAtual,
            motivo,
            criadaEm);
    }
}
