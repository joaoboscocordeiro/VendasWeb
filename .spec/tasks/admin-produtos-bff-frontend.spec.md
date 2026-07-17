spec: task
name: "Produtos administrativos no BFF e frontend"
inherits: project
tags: [bff, frontend, admin, produtos]
---

## Intent

Adicionar uma fachada administrativa de produtos no BFF e uma aba `Produtos` no frontend do PDV para usuarios `ADM`. O recorte reaproveita o CRUD ja existente no servico de CatalogoProdutos e evita que a UI administrativa chame microservicos diretamente.

## Constraints

- As rotas administrativas de produtos no BFF devem exigir JWT com perfil `ADM`.
- Usuarios `VENDEDOR` nao podem listar, criar, editar ou inativar produtos pela fachada administrativa.
- O BFF deve encaminhar o header `Authorization` recebido para CatalogoProdutos.
- A tela `Produtos` deve aparecer somente para usuarios `ADM`.
- Falhas de comunicacao com CatalogoProdutos devem retornar `502`.

## Decisions

- Rotas BFF: `GET /admin/products`, `GET /admin/products/{id}`, `POST /admin/products`, `PUT /admin/products/{id}` e `PATCH /admin/products/{id}/disable`.
- Rotas downstream: CRUD HTTP existente em `/products` no servico CatalogoProdutos.
- Configuracao da base URL: entrada `CatalogoProdutos` em `PdvBootstrap:Servicos`.
- O frontend usa somente `/bff/admin/products` para operacoes administrativas.
- O recorte de estoque permanece fora desta tarefa.

## Boundaries

### Allowed Changes
- .spec/tasks/admin-produtos-bff-frontend.spec.md
- services/Bff/**
- frontend/pdv/**

### Forbidden
- Nao alterar o contrato HTTP do servico CatalogoProdutos.
- Nao acessar diretamente o banco do CatalogoProdutos pelo BFF.
- Nao expor a aba `Produtos` para perfil `VENDEDOR`.
- Nao implementar ajuste de estoque neste recorte.

### Out of Scope
- Movimentacao ou ajuste de estoque.
- Upload/importacao de produtos.
- Paginacao avancada, exportacao ou relatorio de produtos.
- Integracao fiscal ou NFC-e.

## Completion Criteria

Scenario: ADM lista produtos pelo BFF
  Test: bff_admin_can_list_products
  Given existem produtos cadastrados no CatalogoProdutos
  When um cliente com perfil "ADM" chama `GET /admin/products?term=cafe&onlyActive=false`
  Then a resposta tem status "200"
  And o corpo contem os produtos retornados pelo CatalogoProdutos
  And o BFF encaminha o header `Authorization`

Scenario: ADM cria e edita produto pelo BFF
  Test: bff_admin_can_create_and_update_product
  Given um cliente com perfil "ADM"
  When o cliente chama `POST /admin/products` e depois `PUT /admin/products/{id}`
  Then as respostas tem status "201" e "200"
  And o corpo atualizado contem descricao, codigo de barras, preco de custo e preco de venda

Scenario: ADM inativa produto pelo BFF
  Test: bff_admin_can_disable_product
  Given existe um produto ativo
  When um cliente com perfil "ADM" chama `PATCH /admin/products/{id}/disable`
  Then a resposta tem status "204"

Scenario: Vendedor nao acessa administracao de produtos
  Test: bff_seller_cannot_access_admin_products
  Given um cliente com perfil "VENDEDOR"
  When o cliente chama `GET /admin/products`
  Then a resposta tem status "403"

Scenario: CatalogoProdutos indisponivel retorna bad gateway
  Test: bff_admin_products_unavailable_returns_bad_gateway
  Given o servico CatalogoProdutos esta indisponivel
  When um cliente com perfil "ADM" chama `GET /admin/products`
  Then a resposta tem status "502"

Scenario: Frontend exibe produtos somente para ADM
  Test: npm run build
  Given um usuario autenticado com perfil "ADM"
  When o frontend renderiza as areas do sistema
  Then a aba `Produtos` existe e usa `/bff/admin/products`
  And um usuario com perfil "VENDEDOR" permanece sem acesso visual a aba `Produtos`
