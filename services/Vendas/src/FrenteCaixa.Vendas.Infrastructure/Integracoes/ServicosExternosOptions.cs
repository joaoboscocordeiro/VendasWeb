namespace FrenteCaixa.Vendas.Infrastructure.Integracoes;

public sealed class ServicosExternosOptions
{
    public Uri CaixaBaseUrl { get; set; } = new("http://localhost:5062");
    public Uri EstoqueBaseUrl { get; set; } = new("http://localhost:5252");
    public Uri PagamentosBaseUrl { get; set; } = new("http://localhost:5216");
}
