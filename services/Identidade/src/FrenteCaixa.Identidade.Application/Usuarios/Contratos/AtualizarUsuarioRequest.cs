namespace FrenteCaixa.Identidade.Application.Usuarios.Contratos;

public sealed record AtualizarUsuarioRequest(
    string Nome,
    string Email,
    string Perfil);
