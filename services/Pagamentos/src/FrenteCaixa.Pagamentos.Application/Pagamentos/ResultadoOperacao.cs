namespace FrenteCaixa.Pagamentos.Application.Pagamentos;

public enum CodigoErroOperacao
{
    Nenhum = 0,
    Validacao = 1,
    Conflito = 2,
    NaoEncontrado = 3
}

public sealed class ResultadoOperacao<T>
{
    private ResultadoOperacao(bool sucesso, T? valor, CodigoErroOperacao codigoErro, string? erro)
    {
        Sucesso = sucesso;
        Valor = valor;
        CodigoErro = codigoErro;
        Erro = erro;
    }

    public bool Sucesso { get; }
    public T? Valor { get; }
    public CodigoErroOperacao CodigoErro { get; }
    public string? Erro { get; }

    public static ResultadoOperacao<T> Ok(T valor)
    {
        return new ResultadoOperacao<T>(true, valor, CodigoErroOperacao.Nenhum, null);
    }

    public static ResultadoOperacao<T> FalhaValidacao(string erro)
    {
        return new ResultadoOperacao<T>(false, default, CodigoErroOperacao.Validacao, erro);
    }

    public static ResultadoOperacao<T> Conflito(string erro)
    {
        return new ResultadoOperacao<T>(false, default, CodigoErroOperacao.Conflito, erro);
    }

    public static ResultadoOperacao<T> NaoEncontrado(string erro)
    {
        return new ResultadoOperacao<T>(false, default, CodigoErroOperacao.NaoEncontrado, erro);
    }
}
