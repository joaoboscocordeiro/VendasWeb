namespace FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

public interface ISenhaHasher
{
    string GerarHash(string senha);

    bool Verificar(string senha, string senhaHash);
}
