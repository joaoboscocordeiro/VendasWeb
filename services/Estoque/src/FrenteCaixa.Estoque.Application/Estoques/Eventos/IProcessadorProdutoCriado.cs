namespace FrenteCaixa.Estoque.Application.Estoques.Eventos;

public interface IProcessadorProdutoCriado
{
    Task ProcessarAsync(
        Guid mensagemId,
        string routingKey,
        ProdutoCriadoEvento evento,
        CancellationToken cancellationToken);
}
