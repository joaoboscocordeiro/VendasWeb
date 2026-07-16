spec: project
name: "FrenteCaixa Back-end"
tags: [dotnet, csharp, backend, pdv]
---

## Intent

Definir o contrato geral do projeto FrenteCaixa para guiar a criacao de um back-end de PDV em .NET/C#. O projeto deve seguir Clean Architecture, micro-servicos por contexto de negocio, PostgreSQL, Docker, RabbitMQ e autenticacao com JWT.

## Constraints

- O escopo atual e somente back-end.
- A linguagem principal e C#.
- O target framework inicial e `net10.0`.
- A API deve expor contratos HTTP documentaveis por OpenAPI.
- A autenticacao deve usar JWT Bearer Tokens.
- Rotas protegidas devem aplicar autorizacao no limite HTTP.
- Regras de negocio devem ficar fora dos handlers de rota.
- DTOs de entrada e saida nao devem ser reutilizados como entidades de dominio.
- Fluxos que alteram venda e estoque devem preservar consistencia transacional.
- Usuarios e produtos com historico operacional devem ser inativados, nao removidos fisicamente.
- Toda tarefa de implementacao deve ter pelo menos um criterio de aceite testavel.

## Decisions

- Stack inicial: .NET 10, C# e ASP.NET Core Web API.
- Modelo arquitetural: micro-servicos back-end por contexto de negocio.
- Estilo inicial de API: Minimal APIs com handlers finos.
- Organizacao recomendada por servico: API, Application, Domain, Infrastructure e Tests.
- Banco recomendado: PostgreSQL com Entity Framework Core.
- Autenticacao confirmada: JWT Bearer Tokens com perfis `ADM` e `VENDEDOR`.
- Refresh tokens devem ser persistidos com hash, expiracao e revogacao logica.
- Segredos e chaves JWT nao devem ser commitados no repositorio.
- Mensageria confirmada: RabbitMQ.
- Eventos confiaveis devem usar Outbox.
- Front-end: fora do escopo atual.

## Boundaries

### Allowed Changes
- STACK.md
- architecture.md
- .spec/**
- docs/**
- services/**
- src/**
- tests/**
- docker-compose.yml
- *.sln
- *.slnx

### Forbidden
- Nao criar front-end nesta fase.
- Nao gravar senha em texto puro.
- Nao gravar refresh token em texto puro.
- Nao commitar segredos JWT.
- Nao misturar regras de negocio diretamente nos handlers HTTP.

## Out of Scope

- Front-end.
- Aplicativo desktop.
- Integracao real com TEF, Pix ou adquirentes de cartao.
- Observabilidade avancada.
- Deploy em cloud.
- Integracao real com TEF, Pix ou adquirentes.
