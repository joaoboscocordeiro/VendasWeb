namespace FrenteCaixa.Identidade.Application.Usuarios.Contratos;

public sealed record UsuarioResponse(
    Guid Id,
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm);
