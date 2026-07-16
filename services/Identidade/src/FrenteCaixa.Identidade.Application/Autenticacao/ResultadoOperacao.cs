namespace FrenteCaixa.Identidade.Application.Autenticacao;

public sealed class ResultadoOperacao<T>
{
    private ResultadoOperacao(bool sucesso, T? valor, string? erro)
    {
        Sucesso = sucesso;
        Valor = valor;
        Erro = erro;
    }

    public bool Sucesso { get; }

    public T? Valor { get; }

    public string? Erro { get; }

    public static ResultadoOperacao<T> Ok(T valor) => new(true, valor, null);

    public static ResultadoOperacao<T> NaoAutorizado(string erro) => new(false, default, erro);
}
