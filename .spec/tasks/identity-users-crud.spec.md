spec: task
name: "CRUD administrativo de usuarios"
inherits: project
tags: [identidade, usuarios, jwt, api]
---

## Intent

Adicionar ao Servico de Identidade a gestao administrativa de usuarios usada pelo PDV. O login, refresh token e JWT ja existem; este recorte expõe o cadastro, consulta, atualizacao e inativacao logica de usuarios, protegidos para o perfil `ADM`.

## Constraints

- Senhas devem ser persistidas somente como hash.
- A resposta HTTP de usuarios nao deve expor `SenhaHash`.
- Cadastro e manutencao de usuarios devem exigir a policy `SomenteAdministrador`.
- E-mail de usuario deve ser unico.
- Usuarios devem ser inativados logicamente, nao removidos fisicamente.
- Usuario inativo nao pode obter novos tokens no login existente.

## Decisions

- Endpoints administrativos: `POST /users`, `GET /users`, `GET /users/{id}`, `PUT /users/{id}` e `PATCH /users/{id}/disable`.
- Perfis aceitos no contrato HTTP: `ADM` e `VENDEDOR`.
- `POST /users` retorna `201` com o usuario criado sem senha/hash.
- E-mail duplicado retorna `409`.
- Usuario inexistente em consulta, atualizacao ou inativacao retorna `404`.
- Usuario autenticado sem perfil `ADM` recebe `403`.

## Boundaries

### Allowed Changes
- .spec/tasks/identity-users-crud.spec.md
- services/Identidade/**

### Forbidden
- Nao alterar endpoints publicos de login, refresh, logout ou me.
- Nao criar front-end.
- Nao adicionar remocao fisica de usuario.
- Nao commitar segredos JWT.
- Nao expor senha ou hash de senha em contratos HTTP.

## Acceptance Criteria

Scenario: ADM creates user
  Test: identity_admin_can_create_user
  Given an authenticated user with profile "ADM"
  When the client sends a valid request to `POST /users`
  Then the response status code is "201"
  And the response body contains the created user's email
  And the response body does not contain "senhaHash"

Scenario: duplicate email is rejected
  Test: identity_create_user_rejects_duplicate_email
  Given an authenticated user with profile "ADM"
  And an existing user with email "vendedor@frentecaixa.local"
  When the client sends a create request with the same email to `POST /users`
  Then the response status code is "409"

Scenario: seller cannot create user
  Test: identity_seller_cannot_create_user
  Given an authenticated user with profile "VENDEDOR"
  When the client sends a valid request to `POST /users`
  Then the response status code is "403"

Scenario: ADM lists users without password hash
  Test: identity_admin_can_list_users_without_password_hash
  Given an authenticated user with profile "ADM"
  When the client sends `GET /users`
  Then the response status code is "200"
  And the response body contains at least one user
  And the response body does not contain "senhaHash"

Scenario: ADM updates user
  Test: identity_admin_can_update_user
  Given an authenticated user with profile "ADM"
  And an existing user with profile "VENDEDOR"
  When the client sends a valid request to `PUT /users/{id}`
  Then the response status code is "200"
  And the response body contains the updated profile "ADM"

Scenario: missing user returns not found
  Test: identity_update_missing_user_returns_not_found
  Given an authenticated user with profile "ADM"
  When the client sends a valid request to `PUT /users/{id}` with an unknown id
  Then the response status code is "404"

Scenario: disabled user cannot login
  Test: identity_disabled_user_cannot_login_after_admin_disable
  Given an authenticated user with profile "ADM"
  And an active user with email "vendedor@frentecaixa.local"
  When the client sends `PATCH /users/{id}/disable`
  Then the response status code is "204"
  And login for "vendedor@frentecaixa.local" returns status code "401"

## Out of Scope

- Recuperacao ou troca de senha.
- MFA.
- Convite por e-mail.
- API Gateway.
- Propagacao de eventos reais de usuario via RabbitMQ.
- Proteger os demais micro-servicos neste recorte.
