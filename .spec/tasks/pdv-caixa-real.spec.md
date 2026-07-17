spec: task
name: "Caixa real no PDV"
inherits: project
tags: [pdv, bff, caixa, frontend]
---

## Intent

Conectar o PDV ao caixa operacional real antes de habilitar checkout. O operador deve consultar ou abrir seu caixa via BFF, e a criacao de venda no frontend deve usar o `caixaId` retornado pelo servico de Caixa em vez de um identificador digitado ou gerado localmente.

## Constraints

- O servico de Caixa continua sendo o dono das regras de abertura e consulta de caixa.
- O BFF deve exigir usuario com perfil `ADM` ou `VENDEDOR`.
- O BFF deve repassar o bearer token recebido para o servico de Caixa.
- A tela de PDV nao deve permitir iniciar venda sem caixa aberto.
- Regras de negocio devem ficar fora dos handlers HTTP.

## Decisions

- BFF: `GET /pdv/cash-register/current` consulta o caixa atual do operador.
- BFF: `POST /pdv/cash-register/open` abre caixa para o operador autenticado.
- Contrato de abertura no BFF: `valorInicial`.
- Frontend: remove o campo manual `CaixaId` e usa `caixaAtual.id` para criar venda.
- Frontend: mostra uma acao de abertura quando `GET /pdv/cash-register/current` retorna `404`.

## Boundaries

### Allowed Changes
- .spec/tasks/pdv-caixa-real.spec.md
- services/Bff/**
- frontend/pdv/**

### Forbidden
- Nao alterar regras internas do servico de Caixa.
- Nao alterar o endpoint de login.
- Nao implementar checkout ou pagamento neste recorte.
- Nao criar novo banco de dados.
- Nao commitar segredos JWT.

## Acceptance Criteria

Scenario: BFF returns current cash register
  Test: bff_pdv_cash_register_current_returns_operator_register
  Given an authenticated user with profile "VENDEDOR"
  And the Caixa service returns an open cash register
  When the client sends `GET /pdv/cash-register/current`
  Then the response status code is "200"
  And the response contains status "Aberto"

Scenario: BFF returns not found when no cash register is open
  Test: bff_pdv_cash_register_current_returns_not_found_when_missing
  Given an authenticated user with profile "VENDEDOR"
  And the Caixa service returns no open cash register
  When the client sends `GET /pdv/cash-register/current`
  Then the response status code is "404"

Scenario: BFF opens cash register
  Test: bff_pdv_cash_register_open_posts_initial_value
  Given an authenticated user with profile "VENDEDOR"
  When the client sends `POST /pdv/cash-register/open` with `valorInicial` equal to "50.00"
  Then the response status code is "201"
  And the Caixa service receives the same bearer token

Scenario: BFF rejects unauthenticated cash register access
  Test: bff_pdv_cash_register_requires_authentication
  Given no bearer token
  When the client sends `GET /pdv/cash-register/current`
  Then the response status code is "401"

Scenario: Frontend build uses real cash register state
  Test: frontend_pdv_build
  Given the PDV frontend source
  When the frontend build runs
  Then the build succeeds without a manual `CaixaId` input

## Out of Scope

- Finalizacao de venda.
- Registro de pagamento.
- Sangria e suprimento manuais.
- Fechamento de caixa pelo frontend.
