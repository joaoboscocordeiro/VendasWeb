namespace FrenteCaixa.Estoque.Domain.Estoques;

public sealed class MovimentacaoEstoque
{
    private MovimentacaoEstoque()
    {
    }

    private MovimentacaoEstoque(
        Guid id,
        Guid produtoId,
        TipoMovimentacaoEstoque tipo,
        decimal quantidade,
        decimal quantidadeAnterior,
        decimal quantidadeAtual,
        string motivo,
        DateTimeOffset criadaEm)
    {
        Id = id;
        ProdutoId = produtoId;
        Tipo = tipo;
        Quantidade = quantidade;
        QuantidadeAnterior = quantidadeAnterior;
        QuantidadeAtual = quantidadeAtual;
        Motivo = motivo;
        CriadaEm = criadaEm;
    }

    public Guid Id { get; private set; }
    public Guid ProdutoId { get; private set; }
    public TipoMovimentacaoEstoque Tipo { get; private set; }
    public decimal Quantidade { get; private set; }
    public decimal QuantidadeAnterior { get; private set; }
    public decimal QuantidadeAtual { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public DateTimeOffset CriadaEm { get; private set; }

    public static MovimentacaoEstoque Criar(
        Guid produtoId,
        TipoMovimentacaoEstoque tipo,
        decimal quantidade,
        decimal quantidadeAnterior,
        decimal quantidadeAtual,
        string motivo,
        DateTimeOffset criadaEm)
    {
        return new MovimentacaoEstoque(
            Guid.NewGuid(),
            produtoId,
            tipo,
            quantidade,
            quantidadeAnterior,
            quantidadeAtual,
            motivo,
            criadaEm);
    }
}
