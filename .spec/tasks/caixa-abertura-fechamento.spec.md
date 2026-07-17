spec: task
name: "Caixa minimo com abertura e fechamento"
inherits: project
tags: [caixa, jwt, api, postgres]
---

## Intent

Implementar o primeiro recorte real do Servico de Caixa. O servico deve permitir abrir caixa, consultar o caixa aberto do operador, fechar caixa e listar movimentacoes, criando a base operacional necessaria antes de registrar vendas.

## Constraints

- Abertura, consulta e fechamento de caixa devem exigir usuario autenticado.
- Usuarios com perfil `ADM` ou `VENDEDOR` podem abrir caixa.
- Um operador nao pode possuir dois caixas abertos ao mesmo tempo.
- Valor inicial de abertura nao pode ser negativo.
- Valor de fechamento e obrigatorio e nao pode ser negativo.
- Caixa fechado nao pode receber novas movimentacoes.
- Movimentacoes devem preservar tipo, valor, data e descricao.
- Regras de negocio devem ficar fora dos handlers HTTP.

## Decisions

- Endpoints: `POST /cash-registers/open`, `GET /cash-registers/current`, `POST /cash-registers/{id}/close` e `GET /cash-registers/{id}/movements`.
- Banco local: `frente_caixa_caixa`.
- Tabelas principais: `caixas_operacionais` e `movimentacoes_caixa`.
- Status do caixa: `Aberto` e `Fechado`.
- Tipos iniciais de movimentacao: `Abertura` e `Fechamento`.
- Policy de operacao: `OperadorCaixa`.
- Consulta de caixa atual retorna `404` quando o operador autenticado nao possui caixa aberto.
- Eventos RabbitMQ de caixa ficam fora deste recorte.

## Boundaries

### Allowed Changes
- .spec/tasks/caixa-abertura-fechamento.spec.md
- services/Caixa/**

### Forbidden
- Nao implementar venda neste recorte.
- Nao implementar pagamento neste recorte.
- Nao publicar eventos RabbitMQ neste recorte.
- Nao consumir eventos RabbitMQ neste recorte.
- Nao criar front-end.
- Nao alterar endpoints de Identidade, CatalogoProdutos ou Estoque.
- Nao commitar segredos JWT.

## Acceptance Criteria

Scenario: operator opens cash register
  Test: cash_operator_can_open_register
  Given an authenticated user with profile "VENDEDOR"
  When the client sends a valid request to `POST /cash-registers/open`
  Then the response status code is "201"
  And the response body contains status "Aberto"

Scenario: duplicate open register is rejected
  Test: cash_open_rejects_duplicate_open_register
  Given an authenticated user with profile "VENDEDOR"
  And the operator already has an open cash register
  When the client sends another request to `POST /cash-registers/open`
  Then the response status code is "409"

Scenario: invalid opening value is rejected
  Test: cash_open_rejects_negative_initial_value
  Given an authenticated user with profile "VENDEDOR"
  When the client sends an opening request with initial value "-1"
  Then the response status code is "400"

Scenario: authenticated operator gets current cash register
  Test: cash_operator_can_get_current_register
  Given an authenticated user with profile "VENDEDOR"
  And the operator has an open cash register
  When the client sends `GET /cash-registers/current`
  Then the response status code is "200"
  And the response body contains status "Aberto"

Scenario: missing current cash register returns not found
  Test: cash_current_missing_returns_not_found
  Given an authenticated user with profile "VENDEDOR"
  And the operator has no open cash register
  When the client sends `GET /cash-registers/current`
  Then the response status code is "404"

Scenario: operator closes cash register
  Test: cash_operator_can_close_register
  Given an authenticated user with profile "VENDEDOR"
  And the operator has an open cash register
  When the client sends a valid request to `POST /cash-registers/{id}/close`
  Then the response status code is "200"
  And the response body contains status "Fechado"

Scenario: closing another operator register is rejected
  Test: cash_close_rejects_other_operator_register
  Given an authenticated user with profile "VENDEDOR"
  And another operator has an open cash register
  When the client sends a close request to `POST /cash-registers/{id}/close`
  Then the response status code is "404"

Scenario: authenticated operator lists movements
  Test: cash_operator_can_list_register_movements
  Given an authenticated user with profile "VENDEDOR"
  And the operator has an open cash register with movements
  When the client sends `GET /cash-registers/{id}/movements`
  Then the response status code is "200"
  And the response body contains at least one movement

Scenario: current cash register requires authentication
  Test: cash_current_requires_authentication
  Given no access token is provided
  When the client sends `GET /cash-registers/current`
  Then the response status code is "401"

## Out of Scope

- Registro de vendas.
- Registro de pagamentos.
- Sangria e suprimento manuais.
- Publicacao de `caixa.aberto.v1` ou `caixa.fechado.v1`.
- API Gateway.
