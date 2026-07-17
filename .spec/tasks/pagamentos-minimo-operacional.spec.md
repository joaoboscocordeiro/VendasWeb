spec: task
name: "Pagamentos Minimo Operacional"
inherits: project
tags: [pagamentos, pdv, api]
---

## Intent

Implementar o primeiro recorte real do Servico de Pagamentos para registrar a forma de pagamento de uma venda antes do checkout completo. O recorte deve validar dinheiro com troco, registrar cartao e Pix sem integracao externa e persistir os pagamentos no banco proprio `frente_caixa_pagamentos`.

## Decisions

- Rotas HTTP: `POST /payments`, `GET /payments/{id}` e `GET /payments/by-sale/{saleId}`.
- Autorizacao: todas as rotas de pagamento usam policy `OperadorCaixa`, permitindo `ADM` e `VENDEDOR`.
- Formas iniciais: `Dinheiro`, `Cartao` e `Pix`.
- Para `Dinheiro`, `valorPago` deve ser maior ou igual a `valorVenda` e o troco e `valorPago - valorVenda`.
- Para `Cartao` e `Pix`, `valorPago` deve ser igual a `valorVenda` neste recorte.
- Persistencia: Entity Framework Core com PostgreSQL e migrations no projeto `FrenteCaixa.Pagamentos.Infrastructure`.

## Boundaries

### Allowed Changes
- .spec/tasks/pagamentos-minimo-operacional.spec.md
- services/Pagamentos/**

### Forbidden
- Nao alterar contratos dos servicos de Vendas, Estoque, Caixa, CatalogoProdutos, Identidade, Relatorios ou BFF.
- Nao implementar checkout ou finalizacao de venda neste recorte.
- Nao publicar eventos RabbitMQ neste recorte.
- Nao acessar bancos de outros contextos.
- Nao integrar com TEF, Pix real, adquirentes ou gateways externos.

### Out of Scope
- Checkout de venda.
- Baixa ou reserva de estoque.
- Integracao externa de pagamento.
- Estorno, cancelamento ou conciliacao.
- Relatorios financeiros.

## Completion Criteria

Scenario: Operador registra pagamento em dinheiro com troco
  Test: payments_operator_can_register_cash_payment_with_change
  Given um token JWT valido com perfil "VENDEDOR"
  When o cliente chama "POST /payments" com forma "Dinheiro", valorVenda "20.00" e valorPago "50.00"
  Then a resposta tem status "201"
  And o corpo contem troco "30.00" e status "Registrado"

Scenario: Operador registra pagamento com cartao
  Test: payments_operator_can_register_card_payment
  Given um token JWT valido com perfil "VENDEDOR"
  When o cliente chama "POST /payments" com forma "Cartao", valorVenda "42.90" e valorPago "42.90"
  Then a resposta tem status "201"
  And o corpo contem formaPagamento "Cartao" e troco "0"

Scenario: Dinheiro insuficiente e rejeitado
  Test: payments_cash_payment_rejects_insufficient_amount
  Given um token JWT valido com perfil "VENDEDOR"
  When o cliente chama "POST /payments" com forma "Dinheiro", valorVenda "20.00" e valorPago "19.99"
  Then a resposta tem status "400"

Scenario: Cartao com valor divergente e rejeitado
  Test: payments_card_payment_rejects_divergent_amount
  Given um token JWT valido com perfil "VENDEDOR"
  When o cliente chama "POST /payments" com forma "Cartao", valorVenda "20.00" e valorPago "25.00"
  Then a resposta tem status "400"

Scenario: Consulta por venda retorna pagamentos registrados
  Test: payments_operator_can_list_payments_by_sale
  Given existe pagamento registrado para uma venda
  When o cliente chama "GET /payments/by-sale/{saleId}" com token valido
  Then a resposta tem status "200"
  And o corpo contem o pagamento da venda

Scenario: Pagamento exige autenticacao
  Test: payments_requires_authentication
  Given nenhum token JWT
  When o cliente chama "POST /payments"
  Then a resposta tem status "401"
