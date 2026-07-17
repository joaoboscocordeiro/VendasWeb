using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace FrenteCaixa.Bff.Api.Pdv;

public sealed class PdvBootstrapService : IPdvBootstrapService
{
    private readonly IBackendHealthClient _backendHealthClient;
    private readonly PdvBootstrapOptions _options;

    public PdvBootstrapService(
        IBackendHealthClient backendHealthClient,
        IOptions<PdvBootstrapOptions> options)
    {
        _backendHealthClient = backendHealthClient;
        _options = options.Value;
    }

    public async Task<PdvBootstrapResponse> ObterAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken)
    {
        var servicos = _options.Servicos.Select(servico =>
            _backendHealthClient.ObterStatusAsync(servico, cancellationToken));

        return new PdvBootstrapResponse(
            ObterUsuario(usuario),
            new PdvConfiguracaoResponse(
                _options.Moeda,
                _options.CasasDecimais,
                _options.PermiteVendaSemEstoque),
            CriarAtalhos(),
            await Task.WhenAll(servicos),
            DateTimeOffset.UtcNow);
    }

    private static UsuarioBootstrapResponse ObterUsuario(ClaimsPrincipal usuario)
    {
        var sub = usuario.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? usuario.FindFirstValue(ClaimTypes.NameIdentifier);
        var nome = usuario.FindFirstValue("name")
            ?? usuario.FindFirstValue(ClaimTypes.Name)
            ?? "Operador";
        var email = usuario.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? usuario.FindFirstValue(ClaimTypes.Email)
            ?? string.Empty;
        var perfil = usuario.FindFirstValue("role")
            ?? usuario.FindFirstValue(ClaimTypes.Role)
            ?? string.Empty;

        return new UsuarioBootstrapResponse(
            Guid.TryParse(sub, out var usuarioId) ? usuarioId : null,
            nome,
            email,
            perfil);
    }

    private static PdvAtalhoResponse[] CriarAtalhos()
    {
        return
        [
            new("abrir-caixa", "Abrir caixa", "POST", "/cash-registers/open"),
            new("caixa-atual", "Caixa atual", "GET", "/cash-registers/current"),
            new("buscar-produto", "Buscar produto", "GET", "/products"),
            new("registrar-ajuste", "Registrar ajuste", "POST", "/stock/adjustments")
        ];
    }
}
