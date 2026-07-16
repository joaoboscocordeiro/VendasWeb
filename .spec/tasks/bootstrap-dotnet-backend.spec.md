spec: task
name: "Bootstrap .NET Back-end"
inherits: project
tags: [bootstrap, dotnet, api]
---

## Intent

Criar a base inicial do back-end FrenteCaixa usando .NET/C# e ASP.NET Core Web API. A tarefa deve entregar uma solution reconhecivel, com projetos separados por responsabilidade e um endpoint de saude para validar que a API sobe antes de implementar regras de PDV.

## Constraints

- O target framework deve ser `net10.0`.
- O projeto deve continuar backend-only.
- A API deve usar ASP.NET Core Web API.
- O primeiro endpoint deve ser simples e verificavel por HTTP.
- A solution deve permitir adicionar testes automatizados desde o inicio.

## Decisions

- Nome sugerido da solution: `FrenteCaixa`.
- Projetos sugeridos por micro-servico:
  - `services/Identidade/src/FrenteCaixa.Identidade.Api`
  - `services/Identidade/src/FrenteCaixa.Identidade.Application`
  - `services/Identidade/src/FrenteCaixa.Identidade.Domain`
  - `services/Identidade/src/FrenteCaixa.Identidade.Infrastructure`
  - `services/Identidade/tests/FrenteCaixa.Identidade.Tests`
- Micro-servicos iniciais em portugues:
  - `Identidade`
  - `CatalogoProdutos`
  - `Estoque`
  - `Caixa`
  - `Vendas`
  - `Pagamentos`
  - `Relatorios`
- Endpoint inicial: `GET /api/health`.
- Resposta esperada do health check: HTTP 200 com status da aplicacao.
- Validacao minima: `dotnet build` e `dotnet test`.

## Boundaries

### Allowed Changes
- STACK.md
- .spec/**
- services/**
- FrenteCaixa.sln
- FrenteCaixa.slnx

### Forbidden
- Nao implementar cadastro de produto nesta tarefa.
- Nao implementar venda nesta tarefa.
- Nao implementar autenticacao nesta tarefa.
- Nao adicionar banco de dados nesta tarefa.
- Nao criar front-end nesta tarefa.

## Acceptance Criteria

Scenario: solution builds
  Test: dotnet_build_solution
  Given the .NET solution exists at the repository root
  When `dotnet build` is executed
  Then the command exits with code "0"

Scenario: test project runs
  Test: dotnet_test_solution
  Given test projects exist under `services/*/tests`
  When `dotnet test` is executed
  Then the command exits with code "0"

Scenario: health endpoint returns ok
  Test: health_endpoint_returns_200
  Given the API is running locally
  When the client sends `GET /api/health`
  Then the response status code is "200"
  And the response body includes application health status

Scenario: backend-only boundary is preserved
  Test: repository_has_no_frontend_project
  Given the bootstrap task is complete
  When the repository files are inspected
  Then no front-end project exists
  And no UI framework dependency is added

Scenario: PDV features remain out of bootstrap
  Test: bootstrap_does_not_include_pdv_features
  Given the bootstrap task is complete
  When the API endpoints are inspected
  Then product registration endpoints do not exist
  And sale checkout endpoints do not exist
  And authentication endpoints do not exist

Scenario: project names are Portuguese
  Test: bootstrap_uses_portuguese_service_names
  Given the bootstrap task is complete
  When the repository files are inspected
  Then service directories include `Identidade`, `CatalogoProdutos`, `Estoque`, `Caixa`, `Vendas`, `Pagamentos` and `Relatorios`
  And no service directory is named `Identity`, `ProductCatalog`, `Inventory`, `Cashier`, `Sales`, `Payments` or `Reporting`

## Out of Scope

- Cadastro de usuarios.
- Login e JWT.
- Cadastro de produtos.
- Estoque.
- Abertura ou fechamento de caixa.
- Venda e pagamento.
- Banco de dados.
- Docker Compose.
