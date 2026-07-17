spec: task
name: "Checkout no frontend PDV"
inherits: project
tags: [frontend, pdv, vendas, pagamentos, checkout]
---

## Intent

Habilitar a finalizacao de venda na tela do PDV usando o checkout ja existente do servico de Vendas. O operador deve escolher a forma de pagamento, informar o valor recebido quando for dinheiro, concluir a venda e ver o resultado operacional sem alterar venda concluida.

## Constraints

- O checkout deve chamar `POST /sales/{id}/checkout` pelo proxy `/vendas`.
- O frontend deve enviar o bearer token do operador autenticado.
- O botao de checkout deve ficar indisponivel sem venda em andamento com itens.
- Vendas concluidas nao devem permitir adicionar ou remover itens.
- Para `Cartao` e `Pix`, o valor pago enviado deve ser igual ao total da venda.
- Para `Dinheiro`, o valor pago deve vir do campo informado pelo operador.

## Decisions

- Formas iniciais: `Dinheiro`, `Cartao` e `Pix`.
- O frontend calcula o troco exibido para dinheiro como `valorPago - total`.
- O retorno do checkout atualiza o estado da venda com `checkout.venda`.
- O painel de resultado exibe status da venda, pagamentoId e troco calculado.

## Boundaries

### Allowed Changes
- .spec/tasks/frontend-pdv-checkout.spec.md
- frontend/pdv/**

### Forbidden
- Nao alterar regras internas de Vendas, Pagamentos, Estoque ou Caixa.
- Nao criar novos endpoints no BFF neste recorte.
- Nao implementar cancelamento, estorno ou devolucao.
- Nao implementar TEF, Pix real ou adquirente de cartao.

## Acceptance Criteria

Scenario: Frontend builds with checkout controls
  Test: frontend_pdv_build
  Given the PDV frontend source
  When `npm run build` runs
  Then the build succeeds
  And the checkout button is enabled only for an in-progress sale with items

Scenario: Frontend lint accepts checkout code
  Test: frontend_pdv_lint
  Given the PDV frontend source
  When `npm run lint` runs
  Then the lint command succeeds

Scenario: Checkout uses Vendas endpoint
  Test: pdv_checkout_smoke
  Given an authenticated operator with open cash register and a sale with item
  When the frontend submits checkout with payment form "Dinheiro"
  Then the request targets `POST /vendas/sales/{id}/checkout`
  And the sale state becomes "Concluida"

Scenario: Concluded sale blocks item changes
  Test: frontend_pdv_build
  Given a sale with status "Concluida"
  When the screen renders item actions
  Then add and remove item actions are disabled

## Out of Scope

- Fechamento de caixa.
- Sangria e suprimento.
- Integracao real com meios de pagamento.
- Relatorios dentro da tela do PDV.
