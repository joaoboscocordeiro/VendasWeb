spec: task
name: "Implantar RabbitMQ com Outbox e Inbox"
inherits: project
tags: [backend, rabbitmq, outbox, inbox, docker]
---

## Intent

Implantar a base de mensageria do back-end FrenteCaixa para preparar os micro-servicos por contexto com RabbitMQ. Esta tarefa deve adicionar infraestrutura local, contratos compartilhados de eventos, Outbox para publicacao confiavel e Inbox para consumo idempotente, sem implementar ainda os fluxos de produto, estoque ou venda.

## Constraints

- O escopo continua somente back-end.
- RabbitMQ deve rodar via Docker Compose em ambiente local.
- Cada contexto deve ter banco PostgreSQL proprio no ambiente local.
- Eventos devem representar fatos de negocio ja confirmados.
- Outbox deve preservar payload e routing key quando a publicacao falhar.
- Inbox deve permitir detectar mensagem ja consumida pelo identificador do evento.
- Nenhum segredo de producao deve ser commitado.

## Decisions

- Exchange principal: `frente-caixa.eventos`.
- Bancos locais por contexto: `frente_caixa_identidade`, `frente_caixa_catalogo`, `frente_caixa_estoque`, `frente_caixa_caixa`, `frente_caixa_vendas`, `frente_caixa_pagamentos`, `frente_caixa_relatorios`.
- Projeto compartilhado: `building-blocks/FrenteCaixa.BuildingBlocks`.
- RabbitMQ deve usar publisher abstraction antes de ser conectado aos fluxos de negocio.
- Outbox e Inbox entram como modelos e contratos reutilizaveis, sem migracoes especificas por servico nesta tarefa.

## Boundaries

### Allowed Changes
- STACK.md
- architecture.md
- .spec/tasks/rabbitmq-outbox-inbox.spec.md
- docker-compose.yml
- docker/**
- building-blocks/**
- FrenteCaixa.sln
- services/**/appsettings*.json

### Forbidden
- Nao implementar cadastro de produto nesta tarefa.
- Nao implementar deducao de estoque nesta tarefa.
- Nao implementar finalizacao de venda nesta tarefa.
- Nao adicionar front-end.
- Nao commitar credenciais de producao.

## Completion Criteria

Scenario: Docker Compose declara RabbitMQ e bancos por contexto
  Test: docker_compose_declares_rabbitmq_and_context_databases
  Given o arquivo "docker-compose.yml"
  When a configuracao local e lida
  Then existe o servico "rabbitmq"
  And existem os bancos "frente_caixa_identidade", "frente_caixa_catalogo", "frente_caixa_estoque", "frente_caixa_caixa", "frente_caixa_vendas", "frente_caixa_pagamentos" e "frente_caixa_relatorios"

Scenario: Outbox marca evento publicado
  Test: outbox_message_marks_processed_after_publish
  Given uma mensagem Outbox pendente com routing key "catalogo.produto-criado.v1"
  When o processador publica a mensagem com sucesso
  Then a mensagem fica com data de processamento preenchida
  And a quantidade de tentativas permanece "0"

Scenario: Falha de publicacao preserva payload
  Test: outbox_message_records_failure_without_losing_payload
  Given uma mensagem Outbox pendente com payload "{\"id\":\"123\"}"
  When o publicador retorna falha
  Then a mensagem registra a falha
  And o payload permanece "{\"id\":\"123\"}"

Scenario: Inbox identifica mensagem duplicada
  Test: inbox_rejects_duplicate_message
  Given uma mensagem consumida com id "11111111-1111-1111-1111-111111111111"
  When a mesma mensagem chega novamente
  Then o repositorio Inbox informa que ela ja foi processada

## Out of Scope

- Publicar eventos reais de CatalogoProdutos.
- Consumir eventos reais no Estoque.
- Criar migracoes por servico para Outbox e Inbox.
- Subir containers da API no Docker Compose.
