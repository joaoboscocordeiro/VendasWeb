namespace FrenteCaixa.Identidade.Infrastructure.Seguranca;

public sealed class ConfiguracaoJwt
{
    public string Issuer { get; set; } = "FrenteCaixa";

    public string Audience { get; set; } = "FrenteCaixa.Backend";

    public string Chave { get; set; } = string.Empty;

    public int AccessTokenMinutos { get; set; } = 15;
}
