spec: task
name: "Busca de produtos do Catalogo no PDV"
inherits: project
tags: [pdv, bff, catalogo, produtos, frontend]
---

## Intent

Conectar a busca operacional de produtos do PDV ao Catalogo de Produtos. O operador deve localizar produtos ativos por termo ou codigo de barras via BFF e usar o retorno para preencher os dados do item da venda, removendo a dependencia de preenchimento manual no fluxo comum.

## Constraints

- O Catalogo deve continuar sendo o dono dos dados de produto.
- A busca operacional do PDV deve retornar apenas produtos ativos.
- O BFF deve exigir usuario com perfil `ADM` ou `VENDEDOR`.
- O BFF deve repassar o bearer token recebido para o Catalogo.
- O frontend deve preencher `produtoId`, `descricaoProduto` e `precoUnitario` a partir do produto selecionado.
- Regras de negocio devem ficar fora dos handlers HTTP.

## Decisions

- Catalogo: `GET /products` aceita `term` e `onlyActive`.
- BFF: `GET /pdv/products?term=...` retorna produtos ativos para o PDV.
- BFF: `GET /pdv/products/by-barcode/{ean}` retorna um produto ativo ou `404`.
- Contrato do PDV: `id`, `descricao`, `codigoBarrasEan`, `precoVenda` e `ativo`.
- Frontend: a tela de venda usa `/bff/pdv/products` e `/bff/pdv/products/by-barcode/{ean}`.

## Boundaries

### Allowed Changes
- .spec/tasks/pdv-busca-produtos-catalogo.spec.md
- services/CatalogoProdutos/**
- services/Bff/**
- frontend/pdv/**

### Forbidden
- Nao criar novo banco de dados.
- Nao alterar o endpoint de login.
- Nao alterar o contrato de venda em `services/Vendas`.
- Nao implementar cadastro de produtos no frontend.
- Nao commitar segredos JWT.

## Acceptance Criteria

Scenario: Catalog filters active products by term
  Test: catalog_products_query_filters_active_products_by_term
  Given an authenticated user with profile "VENDEDOR"
  And active and inactive products exist
  When the client sends `GET /products?term=cafe&onlyActive=true`
  Then the response status code is "200"
  And the response contains only active products matching "cafe"

Scenario: BFF lists active products for PDV
  Test: bff_pdv_products_returns_active_catalog_products
  Given an authenticated user with profile "VENDEDOR"
  And the Catalogo returns active products
  When the client sends `GET /pdv/products?term=cafe`
  Then the response status code is "200"
  And the response contains product id, description, barcode and sale price

Scenario: BFF rejects unauthenticated product search
  Test: bff_pdv_products_requires_authentication
  Given no bearer token
  When the client sends `GET /pdv/products?term=cafe`
  Then the response status code is "401"

Scenario: BFF returns not found for missing barcode
  Test: bff_pdv_product_by_barcode_returns_not_found_when_missing
  Given an authenticated user with profile "VENDEDOR"
  And the Catalogo has no active product with barcode "7890000000000"
  When the client sends `GET /pdv/products/by-barcode/7890000000000`
  Then the response status code is "404"

Scenario: Frontend fills item fields from selected product
  Test: frontend_pdv_build
  Given the PDV frontend source
  When the frontend build runs
  Then the build succeeds with product search controls present

## Out of Scope

- Cadastro ou edicao de produtos no frontend.
- Consulta de saldo de estoque junto com produto.
- Finalizacao de pagamento.
- Paginacao avancada.
- Leitura fisica de scanner de codigo de barras.
