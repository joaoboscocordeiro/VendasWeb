namespace FrenteCaixa.Caixa.Application.Caixas;

public enum CodigoErroOperacao
{
    Nenhum = 0,
    Validacao = 1,
    Conflito = 2,
    NaoEncontrado = 3
}

public sealed class ResultadoOperacao<T>
{
    private ResultadoOperacao(T? valor, string? erro, CodigoErroOperacao codigoErro)
    {
        Valor = valor;
        Erro = erro;
        CodigoErro = codigoErro;
    }

    public T? Valor { get; }
    public string? Erro { get; }
    public CodigoErroOperacao CodigoErro { get; }
    public bool Sucesso => CodigoErro == CodigoErroOperacao.Nenhum;

    public static ResultadoOperacao<T> Ok(T valor)
    {
        return new ResultadoOperacao<T>(valor, null, CodigoErroOperacao.Nenhum);
    }

    public static ResultadoOperacao<T> FalhaValidacao(string erro)
    {
        return new ResultadoOperacao<T>(default, erro, CodigoErroOperacao.Validacao);
    }

    public static ResultadoOperacao<T> Conflito(string erro)
    {
        return new ResultadoOperacao<T>(default, erro, CodigoErroOperacao.Conflito);
    }

    public static ResultadoOperacao<T> NaoEncontrado(string erro)
    {
        return new ResultadoOperacao<T>(default, erro, CodigoErroOperacao.NaoEncontrado);
    }
}
