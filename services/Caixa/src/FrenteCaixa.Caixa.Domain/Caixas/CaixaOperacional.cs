namespace FrenteCaixa.Caixa.Domain.Caixas;

public sealed class CaixaOperacional
{
    private CaixaOperacional()
    {
    }

    private CaixaOperacional(
        Guid id,
        Guid operadorId,
        decimal valorInicial,
        DateTimeOffset abertoEm)
    {
        Id = id;
        OperadorId = operadorId;
        ValorInicial = valorInicial;
        Status = StatusCaixa.Aberto;
        AbertoEm = abertoEm;
        AtualizadoEm = abertoEm;
    }

    public Guid Id { get; private set; }
    public Guid OperadorId { get; private set; }
    public decimal ValorInicial { get; private set; }
    public decimal? ValorFechamento { get; private set; }
    public StatusCaixa Status { get; private set; }
    public DateTimeOffset AbertoEm { get; private set; }
    public DateTimeOffset? FechadoEm { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }

    public static CaixaOperacional Abrir(Guid operadorId, decimal valorInicial, DateTimeOffset abertoEm)
    {
        return new CaixaOperacional(Guid.NewGuid(), operadorId, valorInicial, abertoEm);
    }

    public MovimentacaoCaixa RegistrarAbertura(DateTimeOffset criadaEm)
    {
        return MovimentacaoCaixa.Criar(
            Id,
            OperadorId,
            TipoMovimentacaoCaixa.Abertura,
            ValorInicial,
            "Abertura de caixa",
            criadaEm);
    }

    public MovimentacaoCaixa Fechar(decimal valorFechamento, DateTimeOffset fechadoEm)
    {
        if (Status == StatusCaixa.Fechado)
        {
            throw new InvalidOperationException("Caixa ja esta fechado.");
        }

        Status = StatusCaixa.Fechado;
        ValorFechamento = valorFechamento;
        FechadoEm = fechadoEm;
        AtualizadoEm = fechadoEm;

        return MovimentacaoCaixa.Criar(
            Id,
            OperadorId,
            TipoMovimentacaoCaixa.Fechamento,
            valorFechamento,
            "Fechamento de caixa",
            fechadoEm);
    }
}
