namespace FrenteCaixa.Relatorios.Domain.Relatorios;

public sealed class ItemVendaConcluidaProjetada
{
    private ItemVendaConcluidaProjetada()
    {
        DescricaoProduto = string.Empty;
    }

    private ItemVendaConcluidaProjetada(
        Guid id,
        Guid vendaId,
        Guid produtoId,
        string descricaoProduto,
        decimal quantidade,
        decimal precoUnitario,
        decimal subtotal)
    {
        Id = id;
        VendaId = vendaId;
        ProdutoId = produtoId;
        DescricaoProduto = descricaoProduto;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        Subtotal = subtotal;
    }

    public Guid Id { get; private set; }
    public Guid VendaId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public string DescricaoProduto { get; private set; }
    public decimal Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal Subtotal { get; private set; }

    public static ItemVendaConcluidaProjetada Criar(
        Guid vendaId,
        Guid produtoId,
        string descricaoProduto,
        decimal quantidade,
        decimal precoUnitario,
        decimal subtotal)
    {
        return new ItemVendaConcluidaProjetada(
            Guid.NewGuid(),
            vendaId,
            produtoId,
            descricaoProduto,
            quantidade,
            precoUnitario,
            subtotal);
    }
}
