spec: task
name: "Resumo real do turno no PDV"
inherits: project
tags: [pdv, caixa, bff, rabbitmq, frontend]
---

## Intent

Adicionar o resumo real do caixa aberto ao fluxo do PDV. O Caixa deve projetar vendas concluidas do evento `vendas.venda-concluida.v1`, calcular totais do turno por forma de pagamento e expor esses totais para o BFF e para a tela antes do fechamento.

## Constraints

- O resumo deve usar dados do proprio contexto Caixa; nao acessar banco de Vendas, Pagamentos ou Relatorios.
- O consumo de `vendas.venda-concluida.v1` deve ser idempotente por `MensagemId`.
- O vendedor so pode consultar resumo de caixa que pertence ao operador autenticado.
- O resumo deve retornar zeros quando o caixa existe e ainda nao possui vendas concluidas projetadas.
- O BFF deve continuar sendo a fachada usada pelo frontend para dados de caixa.

## Decisions

- Evento consumido: `vendas.venda-concluida.v1`.
- Fila RabbitMQ do Caixa: `caixa.vendas.venda-concluida`.
- Endpoint Caixa: `GET /cash-registers/{id}/summary`.
- Endpoint BFF: `GET /pdv/cash-register/current/summary`.
- Totais do resumo: quantidade de vendas, total vendido, dinheiro esperado, total por forma de pagamento, valor inicial e diferenca prevista quando houver valor informado.
- A diferenca prevista usa `valorInformado - (valorInicial + dinheiroVendido)`.

## Boundaries

### Allowed Changes
- .spec/tasks/pdv-resumo-caixa-turno.spec.md
- services/Caixa/**
- services/Bff/**
- frontend/pdv/**

### Forbidden
- Nao alterar o contrato publicado por Vendas para `vendas.venda-concluida.v1`.
- Nao consultar diretamente banco ou API de Relatorios para montar o resumo do Caixa.
- Nao criar integracao real com TEF, Pix ou adquirentes.
- Nao implementar sangria, suprimento ou cancelamento de venda neste recorte.

## Acceptance Criteria

Scenario: Caixa projeta venda concluida
  Test: cash_processor_projects_completed_sale
  Given uma mensagem `vendas.venda-concluida.v1` valida para um caixa existente
  When o processador de Caixa processa a mensagem
  Then a venda aparece na projecao local do Caixa
  And a Inbox registra o message id

Scenario: Mensagem duplicada nao duplica venda
  Test: cash_processor_ignores_duplicate_completed_sale_message
  Given uma mensagem `vendas.venda-concluida.v1` ja registrada na Inbox
  When o processador de Caixa processa a mesma mensagem novamente
  Then existe somente uma venda projetada para o caixa

Scenario: Operador consulta resumo do proprio caixa
  Test: cash_operator_can_get_register_summary
  Given um operador autenticado possui um caixa aberto com vendas projetadas
  When o cliente envia `GET /cash-registers/{id}/summary`
  Then a resposta tem status "200"
  And o corpo contem total vendido, dinheiro esperado e totais por forma de pagamento

Scenario: Resumo de caixa sem vendas retorna zeros
  Test: cash_summary_without_sales_returns_zero_totals
  Given um operador autenticado possui um caixa aberto sem vendas projetadas
  When o cliente envia `GET /cash-registers/{id}/summary`
  Then a resposta tem status "200"
  And o corpo contem quantidade de vendas "0" e total vendido "0"

Scenario: Operador nao consulta caixa de outro operador
  Test: cash_summary_rejects_other_operator_register
  Given outro operador possui um caixa aberto
  When o operador autenticado envia `GET /cash-registers/{id}/summary`
  Then a resposta tem status "404"

Scenario: BFF expoe resumo do caixa atual
  Test: bff_pdv_cash_register_current_summary_returns_summary
  Given o operador autenticado possui caixa atual aberto
  When o frontend chama `GET /pdv/cash-register/current/summary`
  Then a resposta tem status "200"
  And o corpo contem total vendido e totais por forma de pagamento

Scenario: BFF retorna not found sem caixa atual
  Test: bff_pdv_cash_register_current_summary_missing_cash_register_returns_not_found
  Given o operador autenticado nao possui caixa aberto
  When o frontend chama `GET /pdv/cash-register/current/summary`
  Then a resposta tem status "404"

## Out of Scope

- Relatorio administrativo global.
- Historico completo de vendas na tela do PDV.
- Sangria, suprimento, devolucao ou cancelamento.
- Mudanca no payload do evento de Vendas.
