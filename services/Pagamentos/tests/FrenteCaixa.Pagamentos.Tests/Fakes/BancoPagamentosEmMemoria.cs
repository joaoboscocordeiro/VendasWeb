using FrenteCaixa.Pagamentos.Domain.Pagamentos;

namespace FrenteCaixa.Pagamentos.Tests;

public sealed class BancoPagamentosEmMemoria
{
    public List<Pagamento> Pagamentos { get; } = [];
}
