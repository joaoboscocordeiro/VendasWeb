spec: task
name: "Servico de Identidade com JWT"
inherits: project
tags: [identidade, jwt, seguranca, api]
---

## Intent

Implementar o primeiro micro-servico de identidade do FrenteCaixa com login, emissao de JWT, refresh token e autorizacao por perfil. Este servico estabelece a base de seguranca para proteger os demais micro-servicos do PDV.

## Constraints

- O servico deve usar ASP.NET Core Web API em `net10.0`.
- A autenticacao deve usar JWT Bearer Tokens.
- Senhas devem ser armazenadas somente como hash.
- Refresh tokens devem ser armazenados somente como hash.
- Segredos JWT nao podem ser commitados.
- Rotas protegidas devem aplicar autorizacao no limite HTTP.
- O pipeline ASP.NET Core deve executar autenticacao antes de autorizacao.

## Decisions

- Micro-servico: `Servico de Identidade`.
- Perfis iniciais: `ADM` e `VENDEDOR`.
- Endpoints publicos: `POST /auth/login` e `POST /auth/refresh`.
- Endpoint autenticado: `GET /auth/me`.
- Endpoint de logout: `POST /auth/logout`.
- Claims minimas: `sub`, `email`, `role`, `jti`, `iat` e `exp`.
- Falha de autenticacao retorna `401`.
- Falha de autorizacao retorna `403`.

## Boundaries

### Allowed Changes
- architecture.md
- STACK.md
- .spec/**
- services/Identidade/**
- docker-compose.yml

### Forbidden
- Nao implementar cadastro de produto nesta tarefa.
- Nao implementar venda nesta tarefa.
- Nao implementar RabbitMQ nesta tarefa.
- Nao criar front-end nesta tarefa.
- Nao commitar segredos JWT.
- Nao armazenar senha ou refresh token em texto puro.

## Acceptance Criteria

Scenario: login returns JWT for active user
  Test: identity_login_returns_jwt_for_active_user
  Given an active user with profile "VENDEDOR"
  When the client sends valid credentials to `POST /auth/login`
  Then the response status code is "200"
  And the response body contains an access token
  And the response body contains a refresh token

Scenario: invalid credentials are rejected
  Test: identity_login_rejects_invalid_credentials
  Given an active user exists
  When the client sends an invalid password to `POST /auth/login`
  Then the response status code is "401"

Scenario: inactive user cannot login
  Test: identity_login_rejects_inactive_user
  Given an inactive user exists
  When the client sends valid credentials to `POST /auth/login`
  Then the response status code is "401"

Scenario: refresh token renews access token
  Test: identity_refresh_returns_new_access_token
  Given a valid refresh token exists
  When the client sends the refresh token to `POST /auth/refresh`
  Then the response status code is "200"
  And the response body contains a new access token

Scenario: revoked refresh token is rejected
  Test: identity_refresh_rejects_revoked_token
  Given a revoked refresh token exists
  When the client sends the refresh token to `POST /auth/refresh`
  Then the response status code is "401"

Scenario: protected route requires authentication
  Test: identity_me_requires_authentication
  Given no access token is provided
  When the client sends `GET /auth/me`
  Then the response status code is "401"

Scenario: role policy rejects unauthorized profile
  Test: identity_admin_policy_rejects_seller
  Given an authenticated user with profile "VENDEDOR"
  When the client calls an endpoint restricted to "ADM"
  Then the response status code is "403"

## Out of Scope

- Cadastro de produtos.
- Estoque.
- Caixa.
- Venda.
- Pagamento.
- RabbitMQ.
- API Gateway.
- Front-end.
