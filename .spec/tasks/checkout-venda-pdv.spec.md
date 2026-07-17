spec: task
name: "Checkout Venda PDV"
inherits: project
tags: [vendas, checkout, estoque, pagamentos, rabbitmq]
---

## Intent

Implementar o checkout operacional da venda no PDV. O recorte deve transformar uma venda em andamento em venda concluida somente quando houver itens, caixa aberto do operador, deducao de estoque e pagamento registrado.

## Decisions

- Rota de checkout: `POST /sales/{id}/checkout`.
- O checkout usa o token JWT recebido pelo Servico de Vendas para chamar Caixa, Estoque e Pagamentos.
- O Servico de Estoque expoe `POST /stock/deductions` para deducao operacional por venda.
- O Servico de Vendas registra Outbox com routing key `vendas.venda-concluida.v1` apos concluir a venda.
- A venda permanece `EmAndamento` quando qualquer dependencia rejeita ou fica indisponivel.

## Boundaries

### Allowed Changes
- .spec/tasks/checkout-venda-pdv.spec.md
- services/Vendas/**
- services/Estoque/**

### Forbidden
- Nao alterar contratos existentes de login, cadastro de produtos, abertura de caixa ou registro direto de pagamentos.
- Nao acessar bancos de outros contextos diretamente a partir de Vendas.
- Nao implementar relatorios neste recorte.
- Nao criar integracao externa real de Pix, cartao, TEF ou adquirentes.

### Out of Scope
- Consumo de `vendas.venda-concluida.v1` por Relatorios ou Caixa.
- Cancelamento, estorno ou devolucao de venda.
- Saga distribuida completa com compensacao automatica.
- Mudancas no frontend.

## Completion Criteria

Scenario: Operador conclui venda com pagamento e estoque suficiente
  Test: sales_operator_can_checkout_sale
  Given existe uma venda em andamento do operador autenticado com item
  And o caixa da venda esta aberto para o operador
  And o estoque possui saldo suficiente
  When o cliente chama "POST /sales/{id}/checkout" com forma "Dinheiro" e valorPago maior que o total
  Then a resposta tem status "200"
  And o corpo contem venda com status "Concluida"
  And o pagamento foi registrado
  And a deducao de estoque foi solicitada
  And existe Outbox com routing key "vendas.venda-concluida.v1"

Scenario: Venda sem itens nao pode ser concluida
  Test: sales_checkout_rejects_empty_sale
  Given existe uma venda em andamento sem itens
  When o cliente chama "POST /sales/{id}/checkout"
  Then a resposta tem status "400"
  And a venda permanece em status "EmAndamento"

Scenario: Estoque insuficiente rejeita checkout
  Test: sales_checkout_rejects_insufficient_stock
  Given existe uma venda em andamento com item
  And o estoque rejeita a deducao por saldo insuficiente
  When o cliente chama "POST /sales/{id}/checkout"
  Then a resposta tem status "409"
  And a venda permanece em status "EmAndamento"

Scenario: Checkout de venda concluida e rejeitado
  Test: sales_checkout_rejects_already_completed_sale
  Given existe uma venda ja concluida do operador autenticado
  When o cliente chama "POST /sales/{id}/checkout"
  Then a resposta tem status "409"

Scenario: Estoque deduz itens de venda de forma operacional
  Test: stock_operator_can_register_sale_deduction
  Given existe saldo "10" para um produto
  When o cliente chama "POST /stock/deductions" com quantidade "3"
  Then a resposta tem status "201"
  And o saldo do produto passa a ser "7"

Scenario: Deducao operacional nao permite saldo negativo
  Test: stock_sale_deduction_rejects_insufficient_balance
  Given existe saldo "2" para um produto
  When o cliente chama "POST /stock/deductions" com quantidade "3"
  Then a resposta tem status "409"
  And o saldo do produto permanece "2"
