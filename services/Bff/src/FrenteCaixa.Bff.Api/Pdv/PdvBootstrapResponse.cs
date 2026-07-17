namespace FrenteCaixa.Bff.Api.Pdv;

public sealed record PdvBootstrapResponse(
    UsuarioBootstrapResponse Usuario,
    PdvConfiguracaoResponse Configuracao,
    IReadOnlyCollection<PdvAtalhoResponse> Atalhos,
    IReadOnlyCollection<ServicoBackendStatusResponse> Servicos,
    DateTimeOffset GeradoEm);

public sealed record UsuarioBootstrapResponse(
    Guid? Id,
    string Nome,
    string Email,
    string Perfil);

public sealed record PdvConfiguracaoResponse(
    string Moeda,
    int CasasDecimais,
    bool PermiteVendaSemEstoque);

public sealed record PdvAtalhoResponse(
    string Codigo,
    string Rotulo,
    string Metodo,
    string Caminho);

public sealed record ServicoBackendStatusResponse(
    string Nome,
    string Status,
    string BaseUrl,
    int? StatusCode,
    string? Detalhe)
{
    public static ServicoBackendStatusResponse Operacional(string nome, string baseUrl, int statusCode)
    {
        return new ServicoBackendStatusResponse(nome, "Operacional", baseUrl, statusCode, null);
    }

    public static ServicoBackendStatusResponse Indisponivel(
        string nome,
        string baseUrl,
        string detalhe,
        int? statusCode = null)
    {
        return new ServicoBackendStatusResponse(nome, "Indisponivel", baseUrl, statusCode, detalhe);
    }
}
