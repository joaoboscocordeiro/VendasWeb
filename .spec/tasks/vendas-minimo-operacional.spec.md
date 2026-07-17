spec: task
name: "Vendas Minimo Operacional"
inherits: project
tags: [vendas, pdv, api]
---

## Intent

Implementar o primeiro recorte real do Servico de Vendas para sustentar o fluxo de PDV antes do checkout. O servico deve permitir iniciar uma venda em andamento, adicionar e remover itens, consultar totais e persistir a venda no banco proprio `frente_caixa_vendas`.

## Decisions

- Rotas HTTP: `POST /sales`, `POST /sales/{id}/items`, `DELETE /sales/{id}/items/{itemId}`, `GET /sales/{id}`.
- Autorizacao: todas as rotas de venda usam policy `OperadorCaixa`, permitindo `ADM` e `VENDEDOR`.
- Persistencia: Entity Framework Core com PostgreSQL e migrations no projeto `FrenteCaixa.Vendas.Infrastructure`.
- A venda permanece em status `EmAndamento`; checkout, pagamento, baixa de estoque e eventos ficam para recorte posterior.
- Preco unitario informado ao adicionar item deve ser congelado no item da venda.

## Boundaries

### Allowed Changes
- .spec/tasks/vendas-minimo-operacional.spec.md
- services/Vendas/**

### Forbidden
- Nao alterar contratos dos servicos de Caixa, Catalogo, Estoque, Pagamentos ou BFF.
- Nao publicar `vendas.venda-concluida.v1` neste recorte.
- Nao acessar bancos de outros contextos.

### Out of Scope
- Checkout/finalizacao da venda.
- Integracao com pagamento.
- Baixa, reserva ou validacao remota de estoque.
- Consulta de produto em CatalogoProdutos.

## Completion Criteria

Scenario: Operador inicia venda
  Test: sales_operator_can_start_sale
  Given um token JWT valido com perfil "VENDEDOR"
  When o cliente chama "POST /sales" com um caixaId valido
  Then a resposta tem status "201"
  And o corpo contem venda em status "EmAndamento" sem itens e total "0"

Scenario: Operador adiciona item e total e recalculado
  Test: sales_operator_can_add_item_and_total_is_recalculated
  Given existe uma venda em andamento do operador autenticado
  When o cliente chama "POST /sales/{id}/items" com produtoId, descricao, quantidade "2" e precoUnitario "7.50"
  Then a resposta tem status "200"
  And o corpo contem um item com subtotal "15.00"
  And o total da venda e "15.00"

Scenario: Quantidade invalida e rejeitada
  Test: sales_add_item_rejects_invalid_quantity
  Given existe uma venda em andamento do operador autenticado
  When o cliente chama "POST /sales/{id}/items" com quantidade "0"
  Then a resposta tem status "400"

Scenario: Operador remove item e total e recalculado
  Test: sales_operator_can_remove_item_and_total_is_recalculated
  Given existe uma venda em andamento com dois itens
  When o cliente chama "DELETE /sales/{id}/items/{itemId}" para um item
  Then a resposta tem status "200"
  And o item removido nao aparece no corpo
  And o total da venda considera somente o item restante

Scenario: Venda de outro operador nao e exposta
  Test: sales_operator_cannot_get_other_operator_sale
  Given existe uma venda de outro operador
  When o cliente chama "GET /sales/{id}" com token valido de outro operador
  Then a resposta tem status "404"
