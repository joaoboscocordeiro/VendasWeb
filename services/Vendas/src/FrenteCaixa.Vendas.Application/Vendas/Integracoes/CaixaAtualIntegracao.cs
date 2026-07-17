namespace FrenteCaixa.Vendas.Application.Vendas.Integracoes;

public sealed record CaixaAtualIntegracao(
    Guid Id,
    Guid OperadorId,
    string Status);
