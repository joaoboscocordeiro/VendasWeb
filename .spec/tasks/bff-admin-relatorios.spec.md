spec: task
name: "BFF administrativo para relatorios"
inherits: project
tags: [bff, admin, relatorios, api]
---

## Intent

Adicionar ao BFF uma fachada administrativa minima para os relatorios de vendas ja projetados pelo servico de Relatorios. O recorte permite que clientes administrativos consultem vendas concluidas e resumo financeiro sem chamar o microservico diretamente.

## Constraints

- As rotas administrativas devem exigir JWT com perfil `ADM`.
- Usuarios `VENDEDOR` nao podem acessar os relatorios administrativos pelo BFF.
- O BFF deve encaminhar o header `Authorization` recebido para o servico de Relatorios.
- Falhas de comunicacao com Relatorios devem retornar `502`.

## Decisions

- Rotas BFF: `GET /admin/reports/sales` e `GET /admin/reports/financial-summary`.
- Rotas downstream: `GET /reports/sales` e `GET /reports/financial-summary` no servico de Relatorios.
- Configuracao da base URL: entrada `Relatorios` em `PdvBootstrap:Servicos`.
- O BFF define DTOs proprios para a resposta administrativa, sem referenciar o projeto Application de Relatorios.

## Boundaries

### Allowed Changes
- .spec/tasks/bff-admin-relatorios.spec.md
- services/Bff/**

### Forbidden
- Nao alterar o contrato HTTP do servico de Relatorios.
- Nao implementar frontend administrativo neste recorte.
- Nao acessar diretamente o banco de Relatorios.
- Nao criar novas roles alem de `ADM`.

### Out of Scope
- Filtros, paginacao e exportacao.
- Dashboard em tempo real.
- Tela administrativa.
- Relatorios por vendedor autenticado.

## Completion Criteria

Scenario: ADM lista vendas concluidas pelo BFF
  Test: bff_admin_can_list_completed_sales
  Given existe uma venda concluida projetada pelo servico de Relatorios
  When um cliente com perfil "ADM" chama `GET /admin/reports/sales`
  Then a resposta tem status "200"
  And o corpo contem a venda projetada
  And o BFF encaminha o header `Authorization` para Relatorios

Scenario: ADM consulta resumo financeiro pelo BFF
  Test: bff_admin_can_get_financial_summary
  Given existem vendas concluidas projetadas por forma de pagamento
  When um cliente com perfil "ADM" chama `GET /admin/reports/financial-summary`
  Then a resposta tem status "200"
  And o corpo contem quantidade total, valor total e totais por forma de pagamento

Scenario: Vendedor nao acessa relatorios administrativos
  Test: bff_seller_cannot_access_admin_reports
  Given um cliente com perfil "VENDEDOR"
  When o cliente chama `GET /admin/reports/sales`
  Then a resposta tem status "403"

Scenario: Relatorios indisponivel retorna bad gateway
  Test: bff_admin_reports_unavailable_returns_bad_gateway
  Given o servico de Relatorios esta indisponivel
  When um cliente com perfil "ADM" chama `GET /admin/reports/sales`
  Then a resposta tem status "502"
