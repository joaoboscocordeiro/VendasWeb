spec: task
name: "Frontend administrativo de relatorios"
inherits: project
tags: [frontend, admin, relatorios, bff]
---

## Intent

Adicionar ao frontend do PDV uma visao administrativa minima para consultar os relatorios expostos pelo BFF. O recorte deve manter o fluxo de venda como tela principal e permitir que usuarios ADM alternem para resumo financeiro e vendas concluidas.

## Constraints

- A visao de relatorios deve ficar disponivel somente para usuarios com perfil `ADM`.
- Usuarios `VENDEDOR` devem continuar vendo apenas a operacao de vendas.
- As chamadas de relatorios devem usar o proxy `/bff`.
- A tela deve exibir erros retornados pelo BFF sem quebrar a sessao autenticada.

## Decisions

- Endpoints consumidos: `GET /bff/admin/reports/sales` e `GET /bff/admin/reports/financial-summary`.
- Navegacao inicial: `Vendas`.
- A aba `Relatorios` carrega os dados sob demanda quando o ADM acessa ou atualiza a visao.
- O recorte usa React, JSX e CSS puro no app `frontend/pdv`.

## Boundaries

### Allowed Changes
- .spec/tasks/frontend-admin-relatorios.spec.md
- frontend/pdv/**

### Forbidden
- Nao alterar APIs backend neste recorte.
- Nao criar uma segunda aplicacao frontend.
- Nao implementar filtros, paginacao ou exportacao.
- Nao permitir acesso visual de `VENDEDOR` aos relatorios administrativos.

### Out of Scope
- CRUD administrativo.
- Graficos avancados.
- Dashboard em tempo real.
- Relatorios por vendedor autenticado.

## Completion Criteria

Scenario: Frontend compila com tela administrativa
  Test: frontend_pdv_build
  Given o codigo do frontend PDV
  When `npm run build` executa
  Then o build termina com sucesso
  And a tela possui uma visao administrativa para relatorios

Scenario: Frontend lint aceita o recorte
  Test: frontend_pdv_lint
  Given o codigo do frontend PDV
  When `npm run lint` executa
  Then o lint termina com sucesso

Scenario: ADM acessa relatorios pelo BFF
  Test: frontend_admin_reports_contract
  Given um usuario autenticado com perfil "ADM"
  When o usuario abre a visao "Relatorios"
  Then o frontend chama `GET /bff/admin/reports/sales`
  And o frontend chama `GET /bff/admin/reports/financial-summary`

Scenario: Vendedor nao ve a visao administrativa
  Test: frontend_seller_reports_hidden
  Given um usuario autenticado com perfil "VENDEDOR"
  When a aplicacao renderiza a navegacao
  Then a opcao "Relatorios" nao e exibida
