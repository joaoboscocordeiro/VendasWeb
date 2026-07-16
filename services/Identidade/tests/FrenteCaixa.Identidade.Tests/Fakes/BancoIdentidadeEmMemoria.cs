using FrenteCaixa.Identidade.Domain.Usuarios;

namespace FrenteCaixa.Identidade.Tests;

public sealed class BancoIdentidadeEmMemoria
{
    public List<Usuario> Usuarios { get; } = new();

    public List<RefreshToken> RefreshTokens { get; } = new();

    public void Limpar()
    {
        Usuarios.Clear();
        RefreshTokens.Clear();
    }
}
