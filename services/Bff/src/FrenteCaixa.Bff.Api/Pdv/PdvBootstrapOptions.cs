namespace FrenteCaixa.Bff.Api.Pdv;

public sealed class PdvBootstrapOptions
{
    public const string Secao = "PdvBootstrap";

    public string Moeda { get; set; } = "BRL";

    public int CasasDecimais { get; set; } = 2;

    public bool PermiteVendaSemEstoque { get; set; }

    public BackendServicoOptions[] Servicos { get; set; } =
    [
        new() { Nome = "Identidade", BaseUrl = "http://localhost:5227" },
        new() { Nome = "CatalogoProdutos", BaseUrl = "http://localhost:5089" },
        new() { Nome = "Estoque", BaseUrl = "http://localhost:5252" },
        new() { Nome = "Caixa", BaseUrl = "http://localhost:5062" },
        new() { Nome = "Vendas", BaseUrl = "http://localhost:5165" },
        new() { Nome = "Pagamentos", BaseUrl = "http://localhost:5216" },
        new() { Nome = "Relatorios", BaseUrl = "http://localhost:5002" }
    ];
}

public sealed class BackendServicoOptions
{
    public string Nome { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string HealthPath { get; set; } = "/api/saude";
}
