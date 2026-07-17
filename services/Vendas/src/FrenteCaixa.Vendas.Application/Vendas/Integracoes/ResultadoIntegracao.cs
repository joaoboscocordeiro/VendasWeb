namespace FrenteCaixa.Vendas.Application.Vendas.Integracoes;

public enum CodigoErroIntegracao
{
    Nenhum = 0,
    Validacao = 1,
    Conflito = 2,
    NaoEncontrado = 3,
    Indisponivel = 4
}

public sealed class ResultadoIntegracao<T>
{
    private ResultadoIntegracao(bool sucesso, T? valor, CodigoErroIntegracao codigoErro, string? erro)
    {
        Sucesso = sucesso;
        Valor = valor;
        CodigoErro = codigoErro;
        Erro = erro;
    }

    public bool Sucesso { get; }
    public T? Valor { get; }
    public CodigoErroIntegracao CodigoErro { get; }
    public string? Erro { get; }

    public static ResultadoIntegracao<T> Ok(T valor)
    {
        return new ResultadoIntegracao<T>(true, valor, CodigoErroIntegracao.Nenhum, null);
    }

    public static ResultadoIntegracao<T> Falha(CodigoErroIntegracao codigoErro, string erro)
    {
        return new ResultadoIntegracao<T>(false, default, codigoErro, erro);
    }
}
