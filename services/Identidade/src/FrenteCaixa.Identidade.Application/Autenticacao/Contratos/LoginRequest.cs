namespace FrenteCaixa.Identidade.Application.Autenticacao.Contratos;

public sealed record LoginRequest(string Email, string Senha);
