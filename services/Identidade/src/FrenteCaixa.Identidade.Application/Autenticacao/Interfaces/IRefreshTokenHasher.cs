namespace FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

public interface IRefreshTokenHasher
{
    string GerarHash(string token);
}
