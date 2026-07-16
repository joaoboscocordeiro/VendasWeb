using FrenteCaixa.Identidade.Domain.Usuarios;

namespace FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

public interface IGeradorJwt
{
    TokenJwt Gerar(Usuario usuario);
}

public sealed record TokenJwt(string Valor, DateTimeOffset ExpiraEm);
