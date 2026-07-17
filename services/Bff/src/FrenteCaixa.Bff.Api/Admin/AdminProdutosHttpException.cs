using System.Net;

namespace FrenteCaixa.Bff.Api.Admin;

public sealed class AdminProdutosHttpException : Exception
{
    public AdminProdutosHttpException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
