namespace FrenteCaixa.Caixa.Domain.Caixas;

public sealed class MovimentacaoCaixa
{
    private MovimentacaoCaixa()
    {
    }

    private MovimentacaoCaixa(
        Guid id,
        Guid caixaId,
        Guid operadorId,
        TipoMovimentacaoCaixa tipo,
        decimal valor,
        string descricao,
        DateTimeOffset criadaEm)
    {
        Id = id;
        CaixaId = caixaId;
        OperadorId = operadorId;
        Tipo = tipo;
        Valor = valor;
        Descricao = descricao;
        CriadaEm = criadaEm;
    }

    public Guid Id { get; private set; }
    public Guid CaixaId { get; private set; }
    public Guid OperadorId { get; private set; }
    public TipoMovimentacaoCaixa Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public DateTimeOffset CriadaEm { get; private set; }

    public static MovimentacaoCaixa Criar(
        Guid caixaId,
        Guid operadorId,
        TipoMovimentacaoCaixa tipo,
        decimal valor,
        string descricao,
        DateTimeOffset criadaEm)
    {
        return new MovimentacaoCaixa(
            Guid.NewGuid(),
            caixaId,
            operadorId,
            tipo,
            valor,
            descricao,
            criadaEm);
    }
}
