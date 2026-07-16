using System.Security.Cryptography;
using System.Text;
using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

namespace FrenteCaixa.Identidade.Infrastructure.Seguranca;

public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string GerarHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}
