namespace FrenteCaixa.Identidade.Application.Autenticacao.Contratos;

public sealed record AutenticacaoResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiraEm,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiraEm,
    UsuarioAutenticadoResponse Usuario);
