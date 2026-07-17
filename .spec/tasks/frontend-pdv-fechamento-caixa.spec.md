spec: task
name: "Fechamento de caixa no PDV"
inherits: project
tags: [frontend, pdv, bff, caixa]
---

## Intent

Permitir que o operador feche o caixa atual pela tela do PDV depois de concluir vendas. O recorte deve expor fechamento e movimentacoes pelo BFF, mostrar o estado do caixa na tela e impedir novas vendas enquanto nao houver novo caixa aberto.

## Constraints

- O servico de Caixa continua sendo o dono das regras de fechamento e movimentacoes.
- O BFF deve exigir usuario com perfil `ADM` ou `VENDEDOR`.
- O BFF deve repassar o bearer token recebido para o servico de Caixa.
- A tela deve bloquear inicio de nova venda quando o caixa atual estiver fechado ou ausente.
- Valor de fechamento nao pode ser negativo.

## Decisions

- BFF: `GET /pdv/cash-register/current/movements` lista movimentacoes do caixa atual.
- BFF: `POST /pdv/cash-register/{id}/close` fecha o caixa informado.
- Contrato de fechamento no BFF: `valorFechamento`.
- Frontend: fechamento fica dentro da area de caixa operacional, sem criar lateral nova.
- Frontend: apos fechar caixa, `caixaAtual` passa para o retorno fechado e uma nova venda fica bloqueada ate abrir outro caixa.

## Boundaries

### Allowed Changes
- .spec/tasks/frontend-pdv-fechamento-caixa.spec.md
- services/Bff/**
- frontend/pdv/**

### Forbidden
- Nao alterar regras internas do servico de Caixa.
- Nao alterar contratos de login, venda, pagamento ou estoque.
- Nao implementar sangria ou suprimento neste recorte.
- Nao criar novo banco de dados.
- Nao commitar segredos JWT.

## Acceptance Criteria

Scenario: BFF closes current cash register
  Test: bff_pdv_cash_register_close_posts_closing_value
  Given an authenticated user with profile "VENDEDOR"
  And the Caixa service returns an open cash register
  When the client sends `POST /pdv/cash-register/{id}/close` with `valorFechamento` equal to "125.50"
  Then the response status code is "200"
  And the response contains status "Fechado"
  And the Caixa service receives the same bearer token

Scenario: BFF lists current cash register movements
  Test: bff_pdv_cash_register_current_movements_returns_movements
  Given an authenticated user with profile "VENDEDOR"
  And the Caixa service has movements for the current register
  When the client sends `GET /pdv/cash-register/current/movements`
  Then the response status code is "200"
  And the response contains at least one movement

Scenario: BFF rejects invalid closing value
  Test: bff_pdv_cash_register_close_rejects_negative_value
  Given an authenticated user with profile "VENDEDOR"
  When the client sends `POST /pdv/cash-register/{id}/close` with `valorFechamento` equal to "-1"
  Then the response status code is "400"

Scenario: Frontend build includes close cash register flow
  Test: frontend_pdv_build
  Given the PDV frontend source
  When `npm run build` runs
  Then the build succeeds with close cash register controls

## Out of Scope

- Sangria e suprimento.
- Relatorio financeiro do caixa.
- Reabertura de caixa fechado.
- Fechamento automatico por horario.
