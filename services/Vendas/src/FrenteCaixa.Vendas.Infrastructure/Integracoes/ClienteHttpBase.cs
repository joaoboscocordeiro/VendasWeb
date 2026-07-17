using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FrenteCaixa.Vendas.Application.Vendas.Integracoes;

namespace FrenteCaixa.Vendas.Infrastructure.Integracoes;

public abstract class ClienteHttpBase
{
    protected static void AplicarBearer(HttpClient httpClient, string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    protected static async Task<ResultadoIntegracao<T>> MapearRespostaAsync<T>(
        HttpResponseMessage resposta,
        Func<Task<T>> mapearSucesso,
        string mensagemPadrao,
        CancellationToken cancellationToken)
    {
        if (resposta.IsSuccessStatusCode)
        {
            return ResultadoIntegracao<T>.Ok(await mapearSucesso());
        }

        var erro = await LerErroAsync(resposta, mensagemPadrao, cancellationToken);

        return ResultadoIntegracao<T>.Falha(MapearCodigo(resposta.StatusCode), erro);
    }

    protected static ResultadoIntegracao<T> FalhaIndisponivel<T>(string mensagem)
    {
        return ResultadoIntegracao<T>.Falha(CodigoErroIntegracao.Indisponivel, mensagem);
    }

    private static async Task<string> LerErroAsync(
        HttpResponseMessage resposta,
        string mensagemPadrao,
        CancellationToken cancellationToken)
    {
        try
        {
            var erro = await resposta.Content.ReadFromJsonAsync<RespostaErro>(
                cancellationToken: cancellationToken);

            return string.IsNullOrWhiteSpace(erro?.Erro) ? mensagemPadrao : erro.Erro;
        }
        catch
        {
            return mensagemPadrao;
        }
    }

    private static CodigoErroIntegracao MapearCodigo(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => CodigoErroIntegracao.Validacao,
            HttpStatusCode.Conflict => CodigoErroIntegracao.Conflito,
            HttpStatusCode.NotFound => CodigoErroIntegracao.NaoEncontrado,
            _ => CodigoErroIntegracao.Indisponivel
        };
    }

    private sealed record RespostaErro(string Erro);
}
