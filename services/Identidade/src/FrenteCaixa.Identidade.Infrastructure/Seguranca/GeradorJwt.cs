using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;
using FrenteCaixa.Identidade.Domain.Usuarios;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Identidade.Infrastructure.Seguranca;

public sealed class GeradorJwt : IGeradorJwt
{
    private readonly ConfiguracaoJwt _configuracao;
    private readonly IRelogio _relogio;

    public GeradorJwt(IOptions<ConfiguracaoJwt> configuracao, IRelogio relogio)
    {
        _configuracao = configuracao.Value;
        _relogio = relogio;
    }

    public TokenJwt Gerar(Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(_configuracao.Chave))
        {
            throw new InvalidOperationException("A configuracao Jwt:Chave e obrigatoria para gerar tokens.");
        }

        var agora = _relogio.Agora;
        var expiraEm = agora.AddMinutes(_configuracao.AccessTokenMinutos);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, agora.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("role", usuario.Perfil.ToString())
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracao.Chave));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuracao.Issuer,
            audience: _configuracao.Audience,
            claims: claims,
            notBefore: agora.UtcDateTime,
            expires: expiraEm.UtcDateTime,
            signingCredentials: credenciais);

        return new TokenJwt(new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
