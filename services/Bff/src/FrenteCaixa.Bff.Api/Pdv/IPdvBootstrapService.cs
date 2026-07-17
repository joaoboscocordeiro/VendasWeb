using System.Security.Claims;

namespace FrenteCaixa.Bff.Api.Pdv;

public interface IPdvBootstrapService
{
    Task<PdvBootstrapResponse> ObterAsync(ClaimsPrincipal usuario, CancellationToken cancellationToken);
}
