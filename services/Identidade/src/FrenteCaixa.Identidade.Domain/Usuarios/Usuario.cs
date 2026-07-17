namespace FrenteCaixa.Identidade.Domain.Usuarios;

public sealed class Usuario
{
    private Usuario()
    {
        Nome = string.Empty;
        Email = string.Empty;
        SenhaHash = string.Empty;
    }

    private Usuario(
        Guid id,
        string nome,
        string email,
        string senhaHash,
        PerfilUsuario perfil,
        bool ativo,
        DateTimeOffset criadoEm)
    {
        Id = id;
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        Perfil = perfil;
        Ativo = ativo;
        CriadoEm = criadoEm;
        AtualizadoEm = criadoEm;
    }

    public Guid Id { get; private set; }

    public string Nome { get; private set; }

    public string Email { get; private set; }

    public string SenhaHash { get; private set; }

    public PerfilUsuario Perfil { get; private set; }

    public bool Ativo { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public static Usuario Criar(
        string nome,
        string email,
        string senhaHash,
        PerfilUsuario perfil,
        DateTimeOffset criadoEm)
    {
        return new Usuario(Guid.NewGuid(), nome.Trim(), email.Trim().ToLowerInvariant(), senhaHash, perfil, true, criadoEm);
    }

    public void Inativar(DateTimeOffset atualizadoEm)
    {
        Ativo = false;
        AtualizadoEm = atualizadoEm;
    }

    public void AtualizarDados(
        string nome,
        string email,
        PerfilUsuario perfil,
        DateTimeOffset atualizadoEm)
    {
        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        Perfil = perfil;
        AtualizadoEm = atualizadoEm;
    }
}
