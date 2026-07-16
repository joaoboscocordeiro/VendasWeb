namespace FrenteCaixa.Identidade.Domain.Usuarios;

public sealed class RefreshToken
{
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    private RefreshToken(
        Guid id,
        Guid usuarioId,
        string tokenHash,
        DateTimeOffset criadoEm,
        DateTimeOffset expiraEm,
        string? enderecoIpCriacao)
    {
        Id = id;
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        CriadoEm = criadoEm;
        ExpiraEm = expiraEm;
        EnderecoIpCriacao = enderecoIpCriacao;
    }

    public Guid Id { get; private set; }

    public Guid UsuarioId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset ExpiraEm { get; private set; }

    public DateTimeOffset? RevogadoEm { get; private set; }

    public Guid? SubstituidoPorTokenId { get; private set; }

    public string? EnderecoIpCriacao { get; private set; }

    public string? EnderecoIpRevogacao { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public bool EstaExpirado(DateTimeOffset agora) => ExpiraEm <= agora;

    public bool EstaAtivo(DateTimeOffset agora) => RevogadoEm is null && !EstaExpirado(agora);

    public static RefreshToken Criar(
        Guid usuarioId,
        string tokenHash,
        DateTimeOffset criadoEm,
        DateTimeOffset expiraEm,
        string? enderecoIpCriacao)
    {
        return new RefreshToken(Guid.NewGuid(), usuarioId, tokenHash, criadoEm, expiraEm, enderecoIpCriacao);
    }

    public void Revogar(DateTimeOffset revogadoEm, string? enderecoIpRevogacao, Guid? substituidoPorTokenId = null)
    {
        RevogadoEm = revogadoEm;
        EnderecoIpRevogacao = enderecoIpRevogacao;
        SubstituidoPorTokenId = substituidoPorTokenId;
    }
}
