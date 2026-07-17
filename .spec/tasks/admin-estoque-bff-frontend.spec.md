spec: task
name: "Estoque administrativo no BFF e frontend"
inherits: project
tags: [bff, frontend, admin, estoque]
---

## Intent

Adicionar uma fachada administrativa de estoque no BFF e integrar saldo, movimentos e ajuste simples na aba `Produtos` do frontend. O recorte completa o fluxo administrativo iniciado em produtos: o ADM cadastra o produto e consegue consultar/ajustar seu saldo sem chamar o microservico Estoque diretamente.

## Constraints

- As rotas administrativas de estoque no BFF devem exigir JWT com perfil `ADM`.
- Usuarios `VENDEDOR` nao podem consultar saldo, listar movimentos ou registrar ajustes pela fachada administrativa.
- O BFF deve encaminhar o header `Authorization` recebido para Estoque.
- Falhas de comunicacao com Estoque devem retornar `502`.
- O frontend deve expor controles de estoque somente dentro da area `Produtos`, visivel apenas para `ADM`.

## Decisions

- Rotas BFF: `GET /admin/inventory/products/{productId}`, `GET /admin/inventory/products/{productId}/movements` e `POST /admin/inventory/adjustments`.
- Rotas downstream: `GET /stock/products/{productId}`, `GET /stock/products/{productId}/movements` e `POST /stock/adjustments` no servico Estoque.
- Configuracao da base URL: entrada `Estoque` em `PdvBootstrap:Servicos`.
- Ajuste de estoque usa os campos `produtoId`, `tipo`, `quantidade` e `motivo`.
- A tela atualiza saldo e movimentos depois de registrar um ajuste.

## Boundaries

### Allowed Changes
- .spec/tasks/admin-estoque-bff-frontend.spec.md
- services/Bff/**
- frontend/pdv/**

### Forbidden
- Nao alterar o contrato HTTP do servico Estoque.
- Nao acessar diretamente o banco do Estoque pelo BFF.
- Nao expor controles de estoque para perfil `VENDEDOR`.
- Nao implementar importacao/exportacao de estoque neste recorte.

### Out of Scope
- Inventario completo por lote.
- Reserva de estoque.
- Auditoria avancada.
- Regras fiscais ou NFC-e.

## Completion Criteria

Scenario: ADM consulta saldo e movimentos pelo BFF
  Test: bff_admin_can_get_inventory_balance_and_movements
  Given existe saldo e movimento para um produto no Estoque
  When um cliente com perfil "ADM" chama `GET /admin/inventory/products/{productId}` e `GET /admin/inventory/products/{productId}/movements`
  Then as respostas tem status "200"
  And os corpos contem quantidade disponivel e movimentos retornados pelo Estoque
  And o BFF encaminha o header `Authorization`

Scenario: ADM registra ajuste pelo BFF
  Test: bff_admin_can_register_inventory_adjustment
  Given um cliente com perfil "ADM"
  When o cliente chama `POST /admin/inventory/adjustments` com produto, tipo, quantidade e motivo
  Then a resposta tem status "201"
  And o corpo contem a movimentacao de estoque criada

Scenario: Vendedor nao acessa administracao de estoque
  Test: bff_seller_cannot_access_admin_inventory
  Given um cliente com perfil "VENDEDOR"
  When o cliente chama `GET /admin/inventory/products/{productId}`
  Then a resposta tem status "403"

Scenario: Estoque indisponivel retorna bad gateway
  Test: bff_admin_inventory_unavailable_returns_bad_gateway
  Given o servico Estoque esta indisponivel
  When um cliente com perfil "ADM" chama `GET /admin/inventory/products/{productId}`
  Then a resposta tem status "502"

Scenario: Frontend integra estoque na aba Produtos
  Test: npm run build
  Given um usuario autenticado com perfil "ADM"
  When o usuario seleciona um produto na aba `Produtos`
  Then o frontend chama `/bff/admin/inventory/products/{productId}`
  And exibe saldo, movimentos e formulario de ajuste
  And um usuario com perfil "VENDEDOR" permanece sem acesso visual a area `Produtos`
