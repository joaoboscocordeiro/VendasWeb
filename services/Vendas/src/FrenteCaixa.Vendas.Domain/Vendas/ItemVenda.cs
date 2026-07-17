namespace FrenteCaixa.Vendas.Domain.Vendas;

public sealed class ItemVenda
{
    private ItemVenda()
    {
        DescricaoProduto = string.Empty;
    }

    private ItemVenda(
        Guid id,
        Guid vendaId,
        Guid produtoId,
        string descricaoProduto,
        decimal quantidade,
        decimal precoUnitario,
        DateTimeOffset criadoEm)
    {
        Id = id;
        VendaId = vendaId;
        ProdutoId = produtoId;
        DescricaoProduto = descricaoProduto;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        CriadoEm = criadoEm;
    }

    public Guid Id { get; private set; }
    public Guid VendaId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public string DescricaoProduto { get; private set; }
    public decimal Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public decimal Subtotal => Quantidade * PrecoUnitario;

    public static ItemVenda Criar(
        Guid vendaId,
        Guid produtoId,
        string descricaoProduto,
        decimal quantidade,
        decimal precoUnitario,
        DateTimeOffset criadoEm)
    {
        return new ItemVenda(
            Guid.NewGuid(),
            vendaId,
            produtoId,
            descricaoProduto.Trim(),
            quantidade,
            precoUnitario,
            criadoEm);
    }
}
