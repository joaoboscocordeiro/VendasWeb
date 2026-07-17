namespace FrenteCaixa.Caixa.Domain.Caixas;

public sealed class VendaCaixaProjetada
{
    private VendaCaixaProjetada()
    {
        FormaPagamento = string.Empty;
    }

    private VendaCaixaProjetada(
        Guid vendaId,
        Guid caixaId,
        Guid operadorId,
        Guid pagamentoId,
        string formaPagamento,
        decimal valorTotal,
        DateTimeOffset ocorridaEm,
        DateTimeOffset projetadaEm)
    {
        VendaId = vendaId;
        CaixaId = caixaId;
        OperadorId = operadorId;
        PagamentoId = pagamentoId;
        FormaPagamento = formaPagamento;
        ValorTotal = valorTotal;
        OcorridaEm = ocorridaEm;
        ProjetadaEm = projetadaEm;
    }

    public Guid VendaId { get; private set; }
    public Guid CaixaId { get; private set; }
    public Guid OperadorId { get; private set; }
    public Guid PagamentoId { get; private set; }
    public string FormaPagamento { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTimeOffset OcorridaEm { get; private set; }
    public DateTimeOffset ProjetadaEm { get; private set; }

    public static VendaCaixaProjetada Criar(
        Guid vendaId,
        Guid caixaId,
        Guid operadorId,
        Guid pagamentoId,
        string formaPagamento,
        decimal valorTotal,
        DateTimeOffset ocorridaEm,
        DateTimeOffset projetadaEm)
    {
        if (vendaId == Guid.Empty)
        {
            throw new ArgumentException("VendaId e obrigatorio.", nameof(vendaId));
        }

        if (caixaId == Guid.Empty)
        {
            throw new ArgumentException("CaixaId e obrigatorio.", nameof(caixaId));
        }

        if (operadorId == Guid.Empty)
        {
            throw new ArgumentException("OperadorId e obrigatorio.", nameof(operadorId));
        }

        if (pagamentoId == Guid.Empty)
        {
            throw new ArgumentException("PagamentoId e obrigatorio.", nameof(pagamentoId));
        }

        if (string.IsNullOrWhiteSpace(formaPagamento))
        {
            throw new ArgumentException("Forma de pagamento e obrigatoria.", nameof(formaPagamento));
        }

        if (valorTotal <= 0)
        {
            throw new ArgumentException("Valor total deve ser maior que zero.", nameof(valorTotal));
        }

        return new VendaCaixaProjetada(
            vendaId,
            caixaId,
            operadorId,
            pagamentoId,
            formaPagamento.Trim(),
            valorTotal,
            ocorridaEm,
            projetadaEm);
    }
}
