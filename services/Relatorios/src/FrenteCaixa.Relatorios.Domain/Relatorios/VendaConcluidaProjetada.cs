namespace FrenteCaixa.Relatorios.Domain.Relatorios;

public sealed class VendaConcluidaProjetada
{
    private readonly List<ItemVendaConcluidaProjetada> _itens = [];

    private VendaConcluidaProjetada()
    {
        FormaPagamento = string.Empty;
    }

    private VendaConcluidaProjetada(
        Guid vendaId,
        Guid caixaId,
        Guid operadorId,
        Guid pagamentoId,
        string formaPagamento,
        decimal valorTotal,
        DateTimeOffset concluidaEm,
        DateTimeOffset projetadaEm)
    {
        VendaId = vendaId;
        CaixaId = caixaId;
        OperadorId = operadorId;
        PagamentoId = pagamentoId;
        FormaPagamento = formaPagamento;
        ValorTotal = valorTotal;
        ConcluidaEm = concluidaEm;
        ProjetadaEm = projetadaEm;
    }

    public Guid VendaId { get; private set; }
    public Guid CaixaId { get; private set; }
    public Guid OperadorId { get; private set; }
    public Guid PagamentoId { get; private set; }
    public string FormaPagamento { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTimeOffset ConcluidaEm { get; private set; }
    public DateTimeOffset ProjetadaEm { get; private set; }
    public IReadOnlyCollection<ItemVendaConcluidaProjetada> Itens => _itens;

    public static VendaConcluidaProjetada Criar(
        Guid vendaId,
        Guid caixaId,
        Guid operadorId,
        Guid pagamentoId,
        string formaPagamento,
        decimal valorTotal,
        DateTimeOffset concluidaEm,
        DateTimeOffset projetadaEm)
    {
        return new VendaConcluidaProjetada(
            vendaId,
            caixaId,
            operadorId,
            pagamentoId,
            formaPagamento,
            valorTotal,
            concluidaEm,
            projetadaEm);
    }

    public void AdicionarItem(
        Guid produtoId,
        string descricaoProduto,
        decimal quantidade,
        decimal precoUnitario,
        decimal subtotal)
    {
        _itens.Add(ItemVendaConcluidaProjetada.Criar(
            VendaId,
            produtoId,
            descricaoProduto,
            quantidade,
            precoUnitario,
            subtotal));
    }
}
