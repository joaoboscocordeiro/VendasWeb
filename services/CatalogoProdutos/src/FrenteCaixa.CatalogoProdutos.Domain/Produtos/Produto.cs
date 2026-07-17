namespace FrenteCaixa.CatalogoProdutos.Domain.Produtos;

public sealed class Produto
{
    private Produto()
    {
        Descricao = string.Empty;
    }

    private Produto(
        Guid id,
        string descricao,
        string? codigoBarrasEan,
        decimal precoCusto,
        decimal precoVenda,
        bool ativo,
        DateTimeOffset criadoEm)
    {
        Id = id;
        Descricao = descricao;
        CodigoBarrasEan = codigoBarrasEan;
        PrecoCusto = precoCusto;
        PrecoVenda = precoVenda;
        Ativo = ativo;
        CriadoEm = criadoEm;
        AtualizadoEm = criadoEm;
    }

    public Guid Id { get; private set; }

    public string Descricao { get; private set; }

    public string? CodigoBarrasEan { get; private set; }

    public decimal PrecoCusto { get; private set; }

    public decimal PrecoVenda { get; private set; }

    public bool Ativo { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public static Produto Criar(
        string descricao,
        string? codigoBarrasEan,
        decimal precoCusto,
        decimal precoVenda,
        DateTimeOffset criadoEm)
    {
        return new Produto(
            Guid.NewGuid(),
            descricao.Trim(),
            NormalizarCodigoBarras(codigoBarrasEan),
            precoCusto,
            precoVenda,
            true,
            criadoEm);
    }

    public void Atualizar(
        string descricao,
        string? codigoBarrasEan,
        decimal precoCusto,
        decimal precoVenda,
        DateTimeOffset atualizadoEm)
    {
        Descricao = descricao.Trim();
        CodigoBarrasEan = NormalizarCodigoBarras(codigoBarrasEan);
        PrecoCusto = precoCusto;
        PrecoVenda = precoVenda;
        AtualizadoEm = atualizadoEm;
    }

    public void Inativar(DateTimeOffset atualizadoEm)
    {
        Ativo = false;
        AtualizadoEm = atualizadoEm;
    }

    private static string? NormalizarCodigoBarras(string? codigoBarrasEan)
    {
        return string.IsNullOrWhiteSpace(codigoBarrasEan)
            ? null
            : codigoBarrasEan.Trim();
    }
}
