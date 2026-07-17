namespace FrenteCaixa.Pagamentos.Domain.Pagamentos;

public sealed class Pagamento
{
    private Pagamento()
    {
    }

    private Pagamento(
        Guid id,
        Guid vendaId,
        FormaPagamento formaPagamento,
        decimal valorVenda,
        decimal valorPago,
        decimal troco,
        DateTimeOffset criadoEm)
    {
        Id = id;
        VendaId = vendaId;
        FormaPagamento = formaPagamento;
        ValorVenda = valorVenda;
        ValorPago = valorPago;
        Troco = troco;
        Status = StatusPagamento.Registrado;
        CriadoEm = criadoEm;
    }

    public Guid Id { get; private set; }
    public Guid VendaId { get; private set; }
    public FormaPagamento FormaPagamento { get; private set; }
    public decimal ValorVenda { get; private set; }
    public decimal ValorPago { get; private set; }
    public decimal Troco { get; private set; }
    public StatusPagamento Status { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }

    public static Pagamento Registrar(
        Guid vendaId,
        FormaPagamento formaPagamento,
        decimal valorVenda,
        decimal valorPago,
        DateTimeOffset criadoEm)
    {
        var troco = formaPagamento == FormaPagamento.Dinheiro
            ? valorPago - valorVenda
            : 0;

        return new Pagamento(
            Guid.NewGuid(),
            vendaId,
            formaPagamento,
            valorVenda,
            valorPago,
            troco,
            criadoEm);
    }
}
