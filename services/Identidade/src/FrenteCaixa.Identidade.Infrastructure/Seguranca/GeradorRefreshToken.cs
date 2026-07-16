using System.Security.Cryptography;
using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

namespace FrenteCaixa.Identidade.Infrastructure.Seguranca;

public sealed class GeradorRefreshToken : IGeradorRefreshToken
{
    public string Gerar()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
