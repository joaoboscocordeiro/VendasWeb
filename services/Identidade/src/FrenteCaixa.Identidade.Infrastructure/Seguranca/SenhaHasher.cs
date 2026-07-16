using System.Security.Cryptography;
using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

namespace FrenteCaixa.Identidade.Infrastructure.Seguranca;

public sealed class SenhaHasher : ISenhaHasher
{
    private const int Iteracoes = 210_000;
    private const int TamanhoSal = 16;
    private const int TamanhoHash = 32;

    public string GerarHash(string senha)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanhoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return $"pbkdf2-sha256${Iteracoes}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string senha, string senhaHash)
    {
        var partes = senhaHash.Split('$', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length != 4 || partes[0] != "pbkdf2-sha256")
        {
            return false;
        }

        if (!int.TryParse(partes[1], out var iteracoes))
        {
            return false;
        }

        var sal = Convert.FromBase64String(partes[2]);
        var hashEsperado = Convert.FromBase64String(partes[3]);
        var hashInformado = Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, HashAlgorithmName.SHA256, hashEsperado.Length);

        return CryptographicOperations.FixedTimeEquals(hashInformado, hashEsperado);
    }
}
