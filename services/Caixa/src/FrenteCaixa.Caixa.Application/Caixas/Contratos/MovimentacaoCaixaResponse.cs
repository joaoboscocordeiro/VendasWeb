namespace FrenteCaixa.Caixa.Application.Caixas.Contratos;

public sealed record MovimentacaoCaixaResponse(
    Guid Id,
    Guid CaixaId,
    Guid OperadorId,
    string Tipo,
    decimal Valor,
    string Descricao,
    DateTimeOffset CriadaEm);
