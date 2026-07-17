namespace FrenteCaixa.Bff.Api.Pdv;

public sealed record PdvMovimentacaoCaixaResponse(
    Guid Id,
    Guid CaixaId,
    Guid OperadorId,
    string Tipo,
    decimal Valor,
    string Descricao,
    DateTimeOffset CriadaEm);
