namespace FrenteCaixa.Identidade.Application.Autenticacao.Contratos;

public sealed record UsuarioAutenticadoResponse(
    Guid Id,
    string Nome,
    string Email,
    string Perfil);
