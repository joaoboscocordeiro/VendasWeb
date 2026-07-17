using System.Net;

namespace FrenteCaixa.Bff.Api.Pdv;

public sealed class PdvCaixaHttpException : Exception
{
    public PdvCaixaHttpException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
