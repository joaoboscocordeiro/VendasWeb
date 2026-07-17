spec: task
name: "CRUD de catalogo de produtos"
inherits: project
tags: [catalogo, produtos, jwt, api, postgres]
---

## Intent

Implementar o primeiro recorte real do Servico de Catalogo de Produtos. O servico deve permitir cadastrar, consultar, atualizar e inativar produtos, usando PostgreSQL proprio e rotas protegidas por JWT para preparar os fluxos posteriores de estoque e venda.

## Constraints

- Apenas usuarios com perfil `ADM` podem criar, atualizar ou inativar produtos.
- Consultas de produto devem exigir usuario autenticado.
- Produto deve ter descricao obrigatoria.
- Preco de venda deve ser maior que zero.
- Preco de custo nao pode ser negativo.
- Codigo de barras EAN deve ser unico quando informado.
- Produtos devem ser inativados logicamente, nao removidos fisicamente.
- Regras de negocio devem ficar fora dos handlers HTTP.

## Decisions

- Endpoints: `POST /products`, `GET /products`, `GET /products/{id}`, `GET /products/by-barcode/{ean}`, `PUT /products/{id}` e `PATCH /products/{id}/disable`.
- Banco local: `frente_caixa_catalogo`.
- Tabela principal: `produtos`.
- Autorizacao administrativa: policy `SomenteAdministrador`.
- Consultas usam policy `UsuarioAutenticado`.
- Eventos RabbitMQ de catalogo ficam fora deste recorte.

## Boundaries

### Allowed Changes
- .spec/tasks/catalogo-produtos-crud.spec.md
- services/CatalogoProdutos/**

### Forbidden
- Nao implementar estoque neste recorte.
- Nao publicar eventos RabbitMQ neste recorte.
- Nao criar front-end.
- Nao alterar endpoints de Identidade.
- Nao remover produtos fisicamente.
- Nao commitar segredos JWT.

## Acceptance Criteria

Scenario: ADM creates product
  Test: catalog_admin_can_create_product
  Given an authenticated user with profile "ADM"
  When the client sends a valid request to `POST /products`
  Then the response status code is "201"
  And the response body contains the product description

Scenario: seller cannot create product
  Test: catalog_seller_cannot_create_product
  Given an authenticated user with profile "VENDEDOR"
  When the client sends a valid request to `POST /products`
  Then the response status code is "403"

Scenario: duplicate barcode is rejected
  Test: catalog_create_product_rejects_duplicate_barcode
  Given an authenticated user with profile "ADM"
  And an existing product with barcode "7891234567895"
  When the client sends a create request with the same barcode to `POST /products`
  Then the response status code is "409"

Scenario: invalid prices are rejected
  Test: catalog_create_product_rejects_invalid_prices
  Given an authenticated user with profile "ADM"
  When the client sends a request with sale price "0"
  Then the response status code is "400"

Scenario: authenticated user lists products
  Test: catalog_authenticated_user_can_list_products
  Given an authenticated user with profile "VENDEDOR"
  And at least one product exists
  When the client sends `GET /products`
  Then the response status code is "200"
  And the response body contains at least one product

Scenario: ADM updates product
  Test: catalog_admin_can_update_product
  Given an authenticated user with profile "ADM"
  And an active product exists
  When the client sends a valid request to `PUT /products/{id}`
  Then the response status code is "200"
  And the response body contains the updated sale price

Scenario: missing product returns not found
  Test: catalog_update_missing_product_returns_not_found
  Given an authenticated user with profile "ADM"
  When the client sends a valid request to `PUT /products/{id}` with an unknown id
  Then the response status code is "404"

Scenario: ADM disables product
  Test: catalog_admin_can_disable_product
  Given an authenticated user with profile "ADM"
  And an active product exists
  When the client sends `PATCH /products/{id}/disable`
  Then the response status code is "204"
  And `GET /products/{id}` returns the product with active status "false"

## Out of Scope

- Criacao automatica de saldo no Estoque.
- Publicacao de `catalogo.produto-criado.v1`.
- Consumo de eventos no Estoque.
- Integracao com Vendas.
- API Gateway.
