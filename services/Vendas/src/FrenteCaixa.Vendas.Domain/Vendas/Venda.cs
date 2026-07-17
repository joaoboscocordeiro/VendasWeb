namespace FrenteCaixa.Vendas.Domain.Vendas;

public sealed class Venda
{
    private readonly List<ItemVenda> _itens = [];

    private Venda()
    {
    }

    private Venda(Guid id, Guid operadorId, Guid caixaId, DateTimeOffset criadaEm)
    {
        Id = id;
        OperadorId = operadorId;
        CaixaId = caixaId;
        Status = StatusVenda.EmAndamento;
        CriadaEm = criadaEm;
        AtualizadaEm = criadaEm;
    }

    public Guid Id { get; private set; }
    public Guid OperadorId { get; private set; }
    public Guid CaixaId { get; private set; }
    public StatusVenda Status { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }
    public DateTimeOffset AtualizadaEm { get; private set; }
    public IReadOnlyCollection<ItemVenda> Itens => _itens;
    public decimal Total => _itens.Sum(item => item.Subtotal);

    public static Venda Iniciar(Guid operadorId, Guid caixaId, DateTimeOffset criadaEm)
    {
        return new Venda(Guid.NewGuid(), operadorId, caixaId, criadaEm);
    }

    public ItemVenda AdicionarItem(
        Guid produtoId,
        string descricaoProduto,
        decimal quantidade,
        decimal precoUnitario,
        DateTimeOffset criadoEm)
    {
        var item = ItemVenda.Criar(Id, produtoId, descricaoProduto, quantidade, precoUnitario, criadoEm);

        _itens.Add(item);
        AtualizadaEm = criadoEm;

        return item;
    }

    public bool RemoverItem(Guid itemId, DateTimeOffset atualizadoEm)
    {
        var item = _itens.SingleOrDefault(item => item.Id == itemId);

        if (item is null)
        {
            return false;
        }

        _itens.Remove(item);
        AtualizadaEm = atualizadoEm;

        return true;
    }

    public void Concluir(DateTimeOffset concluidaEm)
    {
        Status = StatusVenda.Concluida;
        AtualizadaEm = concluidaEm;
    }
}
