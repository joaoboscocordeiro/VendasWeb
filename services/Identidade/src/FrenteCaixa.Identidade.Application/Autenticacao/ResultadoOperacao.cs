namespace FrenteCaixa.Identidade.Application.Autenticacao;

public sealed class ResultadoOperacao<T>
{
    private ResultadoOperacao(bool sucesso, T? valor, string? erro, CodigoErroOperacao? codigoErro)
    {
        Sucesso = sucesso;
        Valor = valor;
        Erro = erro;
        CodigoErro = codigoErro;
    }

    public bool Sucesso { get; }

    public T? Valor { get; }

    public string? Erro { get; }

    public CodigoErroOperacao? CodigoErro { get; }

    public static ResultadoOperacao<T> Ok(T valor) => new(true, valor, null, null);

    public static ResultadoOperacao<T> FalhaValidacao(string erro) => new(false, default, erro, CodigoErroOperacao.Validacao);

    public static ResultadoOperacao<T> Conflito(string erro) => new(false, default, erro, CodigoErroOperacao.Conflito);

    public static ResultadoOperacao<T> NaoEncontrado(string erro) => new(false, default, erro, CodigoErroOperacao.NaoEncontrado);

    public static ResultadoOperacao<T> NaoAutorizado(string erro) => new(false, default, erro, CodigoErroOperacao.NaoAutorizado);
}

public enum CodigoErroOperacao
{
    Validacao = 1,
    Conflito = 2,
    NaoEncontrado = 3,
    NaoAutorizado = 4
}
