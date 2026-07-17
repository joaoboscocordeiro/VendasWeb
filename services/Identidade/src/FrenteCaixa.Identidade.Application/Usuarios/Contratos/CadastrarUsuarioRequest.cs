namespace FrenteCaixa.Identidade.Application.Usuarios.Contratos;

public sealed record CadastrarUsuarioRequest(
    string Nome,
    string Email,
    string Senha,
    string Perfil);
