spec: task
name: "Orquestracao local dos microsservicos"
inherits: project
tags: [microservicos, local, smoke, scripts, api]
---

## Intent

Criar um caminho operacional unico para subir e validar localmente todos os microsservicos do FrenteCaixa. O script deve iniciar infraestrutura, aplicar migrations quando solicitado, iniciar as APIs nas portas conhecidas e provar readiness por health checks e login/bootstrap basico do PDV.

## Constraints

- A orquestracao deve usar os projetos existentes, sem criar novos servicos.
- PostgreSQL e RabbitMQ devem continuar sendo iniciados pelo `docker-compose.yml`.
- As APIs devem rodar a partir dos assemblies compilados em `bin/Debug/net10.0` apos build unico da solucao.
- Todos os servicos devem compartilhar a mesma `Jwt__Chave` em execucao local.
- O script deve registrar logs fora dos diretorios `bin` e `obj`.
- O comando de parada deve encerrar apenas processos iniciados pelo proprio script.
- O smoke deve retornar exit code diferente de zero se qualquer health check obrigatorio falhar.

## Decisions

- Script principal: `scripts/local/run-microsservicos.ps1`.
- Diretorio de execucao: `.codex-run/frente-caixa-local`.
- Portas: Identidade `5227`, CatalogoProdutos `5089`, Estoque `5252`, Caixa `5062`, Vendas `5165`, Pagamentos `5216`, Relatorios `5002`, BFF `5265`.
- Health check obrigatorio: `GET /api/saude` em cada API.
- Smoke autenticado: login em `POST /auth/login` e bootstrap em `GET /pdv/bootstrap`.
- Usuario dev semeado: `vendedor@frentecaixa.local` com senha `Senha@123`.

## Boundaries

### Allowed Changes
- .spec/tasks/orquestracao-local-microsservicos.spec.md
- scripts/**
- building-blocks/FrenteCaixa.BuildingBlocks.Tests/**

### Forbidden
- Nao alterar contratos HTTP dos microsservicos.
- Nao alterar `docker-compose.yml` para containerizar APIs neste recorte.
- Nao commitar segredos de producao.
- Nao matar processos que nao foram iniciados pelo script.

## Acceptance Criteria

Scenario: Script declares all local APIs
  Test: local_orchestration_script_declares_all_services_and_ports
  Given the script `scripts/local/run-microsservicos.ps1`
  When the script structure is inspected
  Then it lists the eight APIs and their fixed local ports

Scenario: Script supports health-only validation
  Test: local_orchestration_script_supports_health_only_mode
  Given the script `scripts/local/run-microsservicos.ps1`
  When the script structure is inspected
  Then it exposes `-HealthOnly`
  And it checks `/api/saude`

Scenario: Script supports safe stop
  Test: local_orchestration_script_stops_only_recorded_processes
  Given the script `scripts/local/run-microsservicos.ps1`
  When the script structure is inspected
  Then it exposes `-Stop`
  And it reads process ids from `.codex-run/frente-caixa-local/processes.json`

Scenario: Local stack smoke passes
  Test: dotnet_test_solution_and_script_smoke
  Given Docker is available
  When `scripts/local/run-microsservicos.ps1 -SkipBuild -SkipMigrations` runs after a successful build
  Then every API health check returns success
  And `POST /auth/login` returns an access token
  And `GET /pdv/bootstrap` returns services

## Out of Scope

- Containerizar as APIs no Docker Compose.
- Deploy em cloud.
- Observabilidade avancada.
- Teste E2E completo de venda com pagamento.
