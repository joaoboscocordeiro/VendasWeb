# Stack - FrenteCaixa

Este arquivo registra as decisoes de stack do projeto FrenteCaixa. Ele deve ser atualizado sempre que uma decisao tecnica for confirmada.

## Objetivo

Construir o back-end de uma aplicacao de Frente de Caixa (PDV) em .NET/C#, usando Clean Architecture, microsservicos por contexto de negocio, PostgreSQL, Docker e RabbitMQ.

## Decisoes Confirmadas

- Linguagem principal: C#
- Plataforma: .NET 10 LTS
- Target framework inicial: `net10.0`
- SDK de referencia: ultima versao estavel do canal .NET 10 disponivel na maquina e na Microsoft. Em 2026-07-06, a referencia verificada e `10.0.301`.
- Tipo de aplicacao inicial: ASP.NET Core Web API
- Escopo atual: back-end
- Banco de dados: PostgreSQL
- Containerizacao: Docker
- Orquestracao local: Docker Compose
- Mensageria: RabbitMQ
- Exchange RabbitMQ local padrao: `frente-caixa.eventos`
- Autenticacao: JWT Bearer Tokens
- Arquitetura da aplicacao: Clean Architecture
- Arquitetura alvo: micro-servicos back-end por contexto de negocio
- UI/front-end: decidido posteriormente

## Stack Back-end Proposta

- ASP.NET Core Web API para expor endpoints HTTP.
- Minimal APIs para o primeiro ciclo, mantendo handlers finos e regras de negocio fora das rotas.
- Clean Architecture em cada micro-servico:
  - API
  - Application
  - Domain
  - Infrastructure
  - Tests
- DTOs separados dos modelos de dominio e persistencia.
- Respostas de erro no formato ProblemDetails.
- OpenAPI/Swagger habilitado em desenvolvimento.

## Arquitetura de Micro-servicos

- A arquitetura sera back-end only.
- Cada micro-servico deve representar um contexto de negocio claro.
- Cada micro-servico deve possuir sua propria API, camada de Application, Domain, Infrastructure e Tests.
- Cada micro-servico deve ser independente para build, teste e deploy.
- Cada contexto tera banco PostgreSQL proprio, mesmo quando todos os bancos estiverem no mesmo container local.
- A comunicacao sincrona entre servicos deve usar HTTP REST inicialmente.
- A comunicacao assincrona entre servicos deve usar RabbitMQ.
- Eventos publicados no RabbitMQ devem representar fatos de negocio ja confirmados, como venda concluida ou estoque ajustado.
- Evitar micro-servico por tabela. A divisao deve seguir contextos de negocio.

## Clean Architecture

- `Domain` contem entidades, value objects, regras de dominio e eventos de dominio.
- `Application` contem casos de uso, contratos, validacoes de fluxo e interfaces de portas.
- `Infrastructure` contem EF Core, PostgreSQL, RabbitMQ, repositorios, mensageria e integracoes externas.
- `API` contem endpoints HTTP, autenticacao, autorizacao, configuracao do pipeline e serializacao.
- Dependencias devem apontar para dentro: API e Infrastructure dependem de Application/Domain, mas Domain nao depende de nenhuma camada externa.
- Handlers HTTP devem permanecer finos e delegar a regra de negocio para Application.

## Persistencia

- Banco principal confirmado: PostgreSQL.
- ORM recomendado: Entity Framework Core.
- Provider recomendado: Npgsql.EntityFrameworkCore.PostgreSQL.
- Migrations versionadas no repositorio.
- Cada micro-servico deve ter seu proprio banco, schema ou limite de persistencia, evitando compartilhamento direto de tabelas entre contextos.
- Exclusao fisica evitada para registros com historico, como usuarios, produtos e vendas.
- Transacoes obrigatorias no fechamento de venda para garantir consistencia entre venda e estoque.

## Autenticacao e Autorizacao

- Autenticacao confirmada: JWT Bearer Tokens.
- O `Servico de Identidade` sera responsavel por login, emissao, renovacao e revogacao logica de tokens.
- Access tokens devem ter expiracao curta.
- Refresh tokens devem ser persistidos com hash, expiracao e controle de revogacao.
- Tokens JWT devem conter somente claims necessarias, como identificador do usuario, perfil e permissoes relevantes.
- Validacao obrigatoria: issuer, audience, assinatura e expiracao.
- Chaves e segredos JWT nao devem ser commitados no repositorio.
- Em desenvolvimento, segredos devem vir de Secret Manager ou variaveis locais.
- Em producao, segredos devem vir de um cofre seguro ou provedor equivalente.
- Perfis iniciais:
  - `ADM`
  - `VENDEDOR`
- Senhas armazenadas somente com hash seguro.
- Rotas administrativas protegidas por perfil.
- Rotas de PDV protegidas para usuario autenticado com permissao operacional.
- No ASP.NET Core, `UseAuthentication()` deve executar antes de `UseAuthorization()`.
- Minimal APIs devem aplicar `RequireAuthorization()` em grupos de rotas protegidas.

## Dominios Iniciais

- Identidade: usuarios, login, perfis e permissoes.
- CatalogoProdutos: cadastro e consulta de produtos.
- Estoque: saldo e movimentacao de estoque.
- Caixa: abertura e fechamento de caixa.
- Vendas: venda, itens da venda e finalizacao.
- Pagamentos: registro de forma de pagamento.
- Relatorios: consultas consolidadas e relatorios.

## Comunicacao e Integracoes Futuras

- Comunicacao sincrona inicial entre micro-servicos: HTTP REST.
- Comunicacao assincrona confirmada: RabbitMQ.
- Padrao para eventos confiaveis: Outbox.
- Padrao para consumo idempotente de eventos: Inbox, quando necessario.
- Contratos de eventos devem ser versionados.
- Eventos de dominio previstos:
  - `catalogo.produto-criado.v1`
  - `estoque.ajustado.v1`
  - `caixa.aberto.v1`
  - `vendas.venda-concluida.v1`
  - `pagamentos.pagamento-registrado.v1`
  - `caixa.fechado.v1`

## Testes

- Testes de unidade para regras de dominio e casos de uso.
- Testes de integracao para endpoints HTTP principais.
- Testes de persistencia para fluxos que alteram banco de dados.
- Primeiro alvo de validacao: `dotnet test`.
- Build obrigatorio antes de considerar uma tarefa pronta: `dotnet build`.

## Infraestrutura Local

- Docker e Docker Compose para dependencias locais.
- PostgreSQL em container.
- RabbitMQ em container.
- Bancos locais por contexto:
  - `frente_caixa_identidade`
  - `frente_caixa_catalogo`
  - `frente_caixa_estoque`
  - `frente_caixa_caixa`
  - `frente_caixa_vendas`
  - `frente_caixa_pagamentos`
  - `frente_caixa_relatorios`
- Cada micro-servico deve ter Dockerfile proprio quando for criado.
- O `docker-compose.yml` deve subir dependencias locais e, quando fizer sentido, os servicos back-end.
- Variaveis sensiveis fora do repositorio.

## Decisoes Pendentes

- Nome final da solution e dos projetos C#.
- Estrutura exata de pastas da solution.
- Estrategia final de observabilidade.
- Provedor de hospedagem.
- Formato final do front-end.
- Lista inicial definitiva de micro-servicos.
- Padrao final de versionamento dos contratos HTTP e eventos.

## Proxima Decisao Recomendada

Antes de codar regras de negocio, definir a lista inicial enxuta de micro-servicos e criar o primeiro servico .NET com API, Application, Domain, Infrastructure e Tests. Depois disso, implementar o primeiro fluxo real do PDV: cadastro de produto ou abertura/finalizacao de venda.
