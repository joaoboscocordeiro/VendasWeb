spec: task
name: "Relatorios de Vendas Concluidas"
inherits: project
tags: [relatorios, rabbitmq, inbox, api]
---

## Intent

Implementar o primeiro recorte real do Servico de Relatorios a partir do evento `vendas.venda-concluida.v1`. O servico deve consumir o evento de forma idempotente, persistir uma projecao local de vendas concluidas e expor consultas administrativas de vendas e resumo financeiro.

## Decisions

- Evento consumido: `vendas.venda-concluida.v1`.
- Fila RabbitMQ: `relatorios.vendas.venda-concluida`.
- Persistencia local: banco `frente_caixa_relatorios` com tabelas de projecao de vendas e Inbox.
- Endpoints iniciais: `GET /reports/sales` e `GET /reports/financial-summary`.
- Autorizacao: endpoints de relatorio usam policy `SomenteAdministrador`.

## Boundaries

### Allowed Changes
- .spec/tasks/relatorios-vendas-concluidas.spec.md
- services/Relatorios/**

### Forbidden
- Nao alterar contratos de Vendas, Caixa, Estoque, Pagamentos, Identidade ou BFF.
- Nao acessar bancos de outros contextos diretamente.
- Nao modificar o payload de `vendas.venda-concluida.v1`.
- Nao implementar frontend neste recorte.

### Out of Scope
- Consumo de `caixa.fechado.v1`.
- Relatorios por vendedor autenticado.
- Filtros avancados, paginacao e exportacao.
- Dashboards em tempo real.

## Completion Criteria

Scenario: Relatorios projeta venda concluida
  Test: reports_processor_projects_completed_sale
  Given uma mensagem `vendas.venda-concluida.v1` valida
  When o processador de Relatorios processa a mensagem
  Then a venda aparece na projecao local
  And a Inbox registra o message id

Scenario: Mensagem duplicada nao duplica venda
  Test: reports_processor_ignores_duplicate_completed_sale_message
  Given uma mensagem `vendas.venda-concluida.v1` ja registrada na Inbox
  When o processador de Relatorios processa a mesma mensagem novamente
  Then existe somente uma venda projetada

Scenario: Administrador lista vendas concluidas
  Test: reports_admin_can_list_completed_sales
  Given existe uma venda concluida projetada
  When um ADM chama `GET /reports/sales`
  Then a resposta tem status "200"
  And o corpo contem a venda projetada

Scenario: Vendedor nao acessa relatorios globais
  Test: reports_seller_cannot_access_global_sales_report
  Given existe uma venda concluida projetada
  When um VENDEDOR chama `GET /reports/sales`
  Then a resposta tem status "403"

Scenario: Resumo financeiro consolida vendas concluidas
  Test: reports_admin_can_get_financial_summary
  Given existem vendas concluidas projetadas com formas "Dinheiro" e "Pix"
  When um ADM chama `GET /reports/financial-summary`
  Then a resposta tem status "200"
  And o corpo contem quantidade total, valor total e totais por forma de pagamento
