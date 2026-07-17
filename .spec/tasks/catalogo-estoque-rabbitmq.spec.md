spec: task
name: "Fluxo RabbitMQ Catalogo para Estoque"
inherits: project
tags: [rabbitmq, outbox, inbox, catalogo, estoque]
---

## Intent

Conectar o primeiro fluxo assíncrono real entre micro-servicos. Quando um produto for cadastrado no Catalogo, o servico deve registrar `catalogo.produto-criado.v1` em Outbox, publicar no RabbitMQ e permitir que o Estoque consuma o evento de forma idempotente para criar saldo inicial zero.

## Constraints

- O cadastro de produto deve persistir produto e registro Outbox na mesma unidade de trabalho.
- A publicacao deve usar o exchange `frente-caixa.eventos`.
- A routing key do evento deve ser `catalogo.produto-criado.v1`.
- O consumidor do Estoque deve registrar Inbox antes de considerar a mensagem processada.
- Mensagens duplicadas nao podem criar saldo duplicado nem falhar o consumidor.
- Se o saldo do produto ja existir, o consumo deve registrar Inbox e manter o saldo existente.
- Regras de negocio devem ficar fora dos handlers HTTP.

## Decisions

- O evento publicado se chama `ProdutoCriado`.
- A versao do evento e `1`.
- O payload contem `produtoId`, `descricao`, `codigoBarrasEan` e `ocorridoEm`.
- O Catalogo usa tabela `outbox_mensagens`.
- O Estoque usa tabela `inbox_mensagens`.
- A fila do Estoque para este evento se chama `estoque.catalogo.produto-criado`.
- O processamento Outbox usa `OutboxBackgroundService`.
- O consumo do Estoque usa um hosted service especifico para `catalogo.produto-criado.v1`.

## Boundaries

### Allowed Changes
- .spec/tasks/catalogo-estoque-rabbitmq.spec.md
- services/CatalogoProdutos/**
- services/Estoque/**

### Forbidden
- Nao implementar venda neste recorte.
- Nao implementar pagamento neste recorte.
- Nao publicar `estoque.ajustado.v1` neste recorte.
- Nao publicar `caixa.aberto.v1` ou `caixa.fechado.v1` neste recorte.
- Nao criar front-end.
- Nao commitar segredos RabbitMQ ou JWT.

## Acceptance Criteria

Scenario: product creation records outbox event
  Test: catalog_create_product_records_product_created_outbox
  Given an authenticated user with profile "ADM"
  When the client sends a valid request to `POST /products`
  Then the response status code is "201"
  And an Outbox message exists with routing key "catalogo.produto-criado.v1"

Scenario: outbox repository returns pending messages
  Test: catalog_outbox_repository_returns_pending_messages
  Given a pending Outbox message exists
  When the Outbox repository is queried
  Then the pending message is returned

Scenario: stock consumes product created event
  Test: stock_product_created_consumer_creates_zero_balance
  Given a `catalogo.produto-criado.v1` event with a new product id
  When the Stock consumer processes the event
  Then the product balance exists with quantity "0"
  And the Inbox records the message id

Scenario: duplicate product created event is ignored
  Test: stock_product_created_consumer_ignores_duplicate_message
  Given a `catalogo.produto-criado.v1` event message id already exists in Inbox
  When the Stock consumer processes the same event again
  Then no duplicate balance is created

Scenario: existing stock balance is preserved
  Test: stock_product_created_consumer_preserves_existing_balance
  Given a product balance exists with quantity "5"
  When the Stock consumer processes `catalogo.produto-criado.v1` for the same product
  Then the product balance remains quantity "5"
  And the Inbox records the message id

Scenario: docker compose declares RabbitMQ
  Test: docker_compose_declares_rabbitmq
  Given the file `docker-compose.yml`
  When the configuration is read
  Then the service `rabbitmq` exists

## Out of Scope

- Baixa de estoque por venda.
- Publicacao de eventos de estoque.
- Consumo de eventos de venda.
- API Gateway.
