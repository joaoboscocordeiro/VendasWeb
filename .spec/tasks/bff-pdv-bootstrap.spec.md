spec: task
name: "BFF PDV Bootstrap"
inherits: project
tags: [bff, pdv, api]
---

## Intent

Criar o primeiro Backend for Frontend do FrenteCaixa para a experiencia do operador de PDV. O recorte inicial deve expor um bootstrap autenticado que concentra dados seguros do usuario logado, configuracoes basicas do PDV e status dos servicos back-end que a tela precisa observar.

## Decisions

- Novo projeto ASP.NET Core Minimal API: `services/Bff/src/FrenteCaixa.Bff.Api`.
- Novo projeto de testes: `services/Bff/tests/FrenteCaixa.Bff.Tests`.
- Rota inicial: `GET /pdv/bootstrap`.
- Autenticacao: JWT Bearer com as mesmas chaves de configuracao `Jwt:Issuer`, `Jwt:Audience` e `Jwt:Chave` dos outros servicos.
- Autorizacao: policy `OperadorCaixa`, permitindo perfis `ADM` e `VENDEDOR`.
- O BFF deve consultar health endpoints configurados e marcar falhas como `Indisponivel` sem retornar erro 5xx no bootstrap.

## Boundaries

### Allowed Changes
- .spec/tasks/bff-pdv-bootstrap.spec.md
- FrenteCaixa.sln
- services/Bff/**

### Forbidden
- Nao introduzir YARP, reverse proxy generico ou API Gateway neste recorte.
- Nao alterar contratos HTTP dos micro-servicos existentes.
- Nao acessar bancos de dados dos contextos diretamente pelo BFF.

### Out of Scope
- Front-end, telas ou assets visuais.
- Fluxo de venda completo pelo BFF.
- Escrita ou orquestracao transacional entre micro-servicos.

## Completion Criteria

Scenario: Bootstrap autenticado retorna contexto do operador
  Test: bff_bootstrap_returns_operator_context
  Given um token JWT valido com perfil "VENDEDOR"
  When o cliente chama "GET /pdv/bootstrap"
  Then a resposta tem status "200"
  And o corpo contem usuario, perfil, configuracoes do PDV, atalhos e servicos configurados

Scenario: Bootstrap exige usuario autenticado
  Test: bff_bootstrap_requires_authentication
  Given nenhum token JWT
  When o cliente chama "GET /pdv/bootstrap"
  Then a resposta tem status "401"

Scenario: Perfil sem permissao nao acessa bootstrap
  Test: bff_bootstrap_rejects_unauthorized_role
  Given um token JWT valido com perfil "SUPORTE"
  When o cliente chama "GET /pdv/bootstrap"
  Then a resposta tem status "403"

Scenario: Falha parcial de backend nao quebra bootstrap
  Test: bff_bootstrap_marks_unavailable_backend_without_5xx
  Given um backend configurado retorna falha ao health check
  When o cliente chama "GET /pdv/bootstrap" com token valido
  Then a resposta tem status "200"
  And o servico com falha aparece com status "Indisponivel"
