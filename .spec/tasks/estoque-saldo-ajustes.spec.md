spec: task
name: "Estoque minimo com saldo e ajustes"
inherits: project
tags: [estoque, saldo, jwt, api, postgres]
---

## Intent

Implementar o primeiro recorte real do Servico de Estoque. O servico deve permitir consultar saldo por produto, listar movimentacoes e registrar ajustes manuais de entrada ou saida, criando a base operacional necessaria antes de implementar vendas.

## Constraints

- Consultas de estoque devem exigir usuario autenticado.
- Apenas usuarios com perfil `ADM` podem registrar ajustes manuais.
- Quantidade de ajuste deve ser maior que zero.
- Ajuste de saida nao pode deixar saldo negativo.
- Produto e referenciado por `productId` do Catalogo, sem chamada HTTP ao Catalogo neste recorte.
- Movimentacoes devem preservar quantidade anterior, quantidade atual, tipo, motivo e data.
- Regras de negocio devem ficar fora dos handlers HTTP.

## Decisions

- Endpoints: `GET /stock/products/{productId}`, `GET /stock/products/{productId}/movements` e `POST /stock/adjustments`.
- Banco local: `frente_caixa_estoque`.
- Tabelas principais: `saldos_produtos` e `movimentacoes_estoque`.
- Consulta de saldo sem movimentacao retorna quantidade "0".
- Tipos aceitos para ajuste: `Entrada` e `Saida`.
- Autorizacao administrativa: policy `SomenteAdministrador`.
- Consultas usam policy `UsuarioAutenticado`.
- Eventos RabbitMQ de estoque ficam fora deste recorte.

## Boundaries

### Allowed Changes
- .spec/tasks/estoque-saldo-ajustes.spec.md
- services/Estoque/**

### Forbidden
- Nao implementar venda neste recorte.
- Nao publicar eventos RabbitMQ neste recorte.
- Nao consumir eventos RabbitMQ neste recorte.
- Nao criar front-end.
- Nao alterar endpoints de Identidade ou CatalogoProdutos.
- Nao commitar segredos JWT.

## Acceptance Criteria

Scenario: ADM registers entry adjustment
  Test: stock_admin_can_register_entry_adjustment
  Given an authenticated user with profile "ADM"
  When the client sends a valid entry adjustment to `POST /stock/adjustments`
  Then the response status code is "201"
  And `GET /stock/products/{productId}` returns quantity "10"

Scenario: seller cannot register adjustment
  Test: stock_seller_cannot_register_adjustment
  Given an authenticated user with profile "VENDEDOR"
  When the client sends a valid adjustment to `POST /stock/adjustments`
  Then the response status code is "403"

Scenario: invalid quantity is rejected
  Test: stock_adjustment_rejects_invalid_quantity
  Given an authenticated user with profile "ADM"
  When the client sends an adjustment with quantity "0"
  Then the response status code is "400"

Scenario: output cannot make stock negative
  Test: stock_output_rejects_negative_stock
  Given an authenticated user with profile "ADM"
  And the product balance is "3"
  When the client sends an output adjustment with quantity "5"
  Then the response status code is "409"

Scenario: authenticated user gets balance
  Test: stock_authenticated_user_can_get_balance
  Given an authenticated user with profile "VENDEDOR"
  And a product balance exists with quantity "7"
  When the client sends `GET /stock/products/{productId}`
  Then the response status code is "200"
  And the response body contains quantity "7"

Scenario: authenticated user lists movements
  Test: stock_authenticated_user_can_list_movements
  Given an authenticated user with profile "VENDEDOR"
  And a product has at least one stock movement
  When the client sends `GET /stock/products/{productId}/movements`
  Then the response status code is "200"
  And the response body contains at least one movement

Scenario: balance requires authentication
  Test: stock_balance_requires_authentication
  Given no access token is provided
  When the client sends `GET /stock/products/{productId}`
  Then the response status code is "401"

## Out of Scope

- Integracao sincrona com CatalogoProdutos.
- Publicacao de `estoque.ajustado.v1`.
- Consumo de `catalogo.produto-criado.v1`.
- Reserva ou baixa automatica de estoque por venda.
- API Gateway.
