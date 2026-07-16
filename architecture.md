# Arquitetura Back-end - FrenteCaixa

Documento criado a partir do arquivo `C:\Users\Junior\Documents\PROJETOS\Frente de Caixa.docx` e alinhado ao `STACK.md`.

## Objetivo

Definir as regras de negocio, dominios e fluxo de micro-servicos para o back-end de uma aplicacao de Frente de Caixa (PDV).

O foco atual e somente back-end. Front-end, telas, aplicativo desktop e deploy em producao ficam fora deste documento inicial.

## Stack Arquitetural

- Linguagem: C#
- Plataforma: .NET 10 LTS
- Aplicacao: ASP.NET Core Web API
- Arquitetura: Clean Architecture
- Banco de dados: PostgreSQL
- Containerizacao: Docker
- Orquestracao local: Docker Compose
- Mensageria: RabbitMQ
- Autenticacao: JWT Bearer Tokens
- Comunicacao sincrona: HTTP REST
- Comunicacao assincrona: eventos via RabbitMQ
- Persistencia por contexto: cada micro-servico deve ter banco, schema ou limite de persistencia proprio

## Regras de Negocio

### Acesso e Usuarios

- O sistema deve autenticar usuarios por login/e-mail e senha.
- A senha deve ser armazenada somente como hash seguro.
- O login/e-mail do usuario deve ser unico.
- O sistema deve reconhecer dois perfis iniciais: `ADM` e `VENDEDOR`.
- Login bem-sucedido deve gerar access token JWT.
- Refresh token deve ser persistido de forma segura para permitir renovacao de sessao.
- Access token expirado deve ser recusado pelos micro-servicos protegidos.
- Usuario inativo nao pode obter novos tokens.
- Refresh token revogado ou expirado nao pode gerar novo access token.
- Usuario `ADM` pode cadastrar usuarios, produtos, alterar estoque, alterar precos e consultar relatorios gerais.
- Usuario `VENDEDOR` pode operar o PDV, abrir caixa, realizar vendas, fechar caixa e consultar o proprio turno.
- Usuario `VENDEDOR` nao pode alterar estoque, precos, produtos ou usuarios.
- Usuarios vinculados a vendas historicas nao devem ser removidos fisicamente; devem ser inativados.

### Produtos

- Apenas usuario `ADM` pode cadastrar, editar ou inativar produtos.
- Produto deve possuir identificador unico.
- Codigo de barras EAN deve ser unico quando informado.
- Produto deve conter descricao/nome, preco de custo, preco de venda e quantidade em estoque.
- Preco de venda deve ser maior que zero.
- O sistema pode sugerir preco de venda baseado em custo e margem, mas a decisao final pertence ao `ADM`.
- Produto disponivel para venda deve estar ativo e possuir estoque suficiente.
- Produtos vendidos anteriormente nao devem ser removidos fisicamente; devem ser inativados para manter historico.

### Estoque

- O estoque nao pode ficar negativo.
- A entrada inicial de estoque pode ocorrer no cadastro do produto.
- Toda alteracao de estoque deve gerar movimento rastreavel.
- A deducao definitiva do estoque ocorre somente quando a venda e finalizada com sucesso.
- Se a finalizacao da venda falhar, o estoque nao deve ser deduzido.
- Se a deducao do estoque falhar, a venda nao deve ser confirmada.

### Caixa

- Vendedor deve abrir caixa antes de registrar vendas.
- A abertura de caixa deve registrar vendedor, data/hora e valor inicial em dinheiro.
- Um vendedor nao deve ter mais de um caixa aberto ao mesmo tempo.
- Toda venda deve estar vinculada a um caixa aberto.
- No fechamento, o sistema deve somar as vendas do periodo e confrontar o dinheiro esperado com o dinheiro fisico informado.
- Diferencas de caixa devem ser registradas para auditoria.
- Caixa fechado nao pode receber novas vendas.

### Venda

- Venda deve estar vinculada a um vendedor autenticado e a um caixa aberto.
- Itens podem ser adicionados por ID do produto ou codigo de barras.
- Ao adicionar item, o sistema deve consultar produto ativo, preco atual e disponibilidade de estoque.
- O preco unitario da venda deve ser congelado no momento em que o item entra na venda.
- A venda deve registrar produto, quantidade, preco unitario, subtotal, valor total e forma de pagamento.
- O total da venda deve ser calculado pela soma dos subtotais dos itens.
- A venda so pode ser finalizada se todos os itens tiverem estoque suficiente.
- Ao finalizar a venda, o sistema deve registrar a venda, registrar os itens, registrar o pagamento e deduzir estoque.
- A finalizacao da venda deve ser consistente: venda confirmada e estoque deduzido devem ocorrer como uma unica operacao de negocio.

### Pagamento

- Formas iniciais de pagamento: dinheiro, cartao e Pix.
- A forma de pagamento deve ser informada no fechamento da venda.
- Para dinheiro, o valor pago deve ser igual ou maior que o total da venda.
- Para dinheiro, o sistema deve calcular troco quando houver valor excedente.
- Para cartao e Pix, a primeira versao registra apenas a forma e o valor; integracoes externas ficam para fase posterior.

### Relatorios

- Usuario `ADM` pode consultar vendas e fechamento de caixa de todos os vendedores.
- Usuario `VENDEDOR` pode consultar apenas o proprio historico ou o proprio turno.
- Relatorios financeiros devem considerar vendas confirmadas.
- Cancelamentos e ajustes futuros devem aparecer separados para auditoria.

## Dominios

### Identidade e Acesso

Responsavel por usuarios, credenciais, perfis, autenticacao e autorizacao.

Entidades principais:

- Usuario
- Perfil
- Credencial
- TokenSessao

Casos de uso principais:

- autenticar usuario;
- emitir JWT;
- renovar JWT usando refresh token valido;
- revogar refresh token no logout;
- cadastrar usuario;
- inativar usuario;
- validar permissao por perfil.

## Autenticacao JWT

### Responsabilidades do Servico de Identidade

- Validar credenciais de login.
- Emitir access token JWT.
- Emitir refresh token.
- Persistir refresh token com hash, expiracao, usuario, data de criacao e data de revogacao.
- Renovar access token quando o refresh token for valido.
- Revogar refresh token no logout.
- Impedir emissao de token para usuario inativo.

### Claims Minimas

- `sub`: identificador do usuario.
- `email`: login/e-mail do usuario.
- `role`: perfil do usuario, inicialmente `ADM` ou `VENDEDOR`.
- `jti`: identificador unico do token.
- `iat`: data/hora de emissao.
- `exp`: data/hora de expiracao.

### Validacoes Obrigatorias

- Assinatura do token.
- Emissor, `issuer`.
- Audiencia, `audience`.
- Expiracao.
- Usuario ativo quando a operacao exigir consulta de seguranca atual.
- Perfil necessario para a rota acessada.

### Politicas de Autorizacao

- `SomenteAdministrador`: permite somente `ADM`.
- `SomenteVendedor`: permite somente `VENDEDOR`.
- `UsuarioAutenticado`: permite qualquer usuario autenticado.
- `OperadorCaixa`: permite `VENDEDOR` e, quando necessario, `ADM`.

### Rotas Publicas

- `POST /auth/login`
- `POST /auth/refresh`

### Rotas Protegidas por Perfil

- Cadastro e manutencao de usuarios: `SomenteAdministrador`.
- Cadastro e manutencao de produtos: `SomenteAdministrador`.
- Ajustes de estoque: `SomenteAdministrador`.
- Relatorios globais: `SomenteAdministrador`.
- Abertura de caixa: `SomenteVendedor` ou `OperadorCaixa`.
- Venda no PDV: `SomenteVendedor` ou `OperadorCaixa`.
- Consulta do proprio turno: `UsuarioAutenticado` com filtro pelo usuario autenticado.

### Propagacao entre Micro-servicos

- Chamadas HTTP entre micro-servicos devem propagar o access token quando a operacao depender do usuario logado.
- Cada micro-servico deve validar o JWT nas rotas protegidas.
- Autorizacao deve ser aplicada no limite do endpoint, usando `RequireAuthorization()` ou equivalente.
- Comunicacoes internas sem usuario final devem usar mecanismo separado de credencial de servico, a ser definido depois.

### Catalogo de Produtos

Responsavel pelos dados comerciais dos produtos.

Entidades principais:

- Produto
- CodigoBarras
- PrecoProduto

Casos de uso principais:

- cadastrar produto;
- editar produto;
- inativar produto;
- consultar produto por ID;
- consultar produto por EAN;
- disponibilizar preco atual para venda.

### Estoque

Responsavel por saldos e movimentos de estoque.

Entidades principais:

- SaldoEstoque
- MovimentoEstoque
- ReservaEstoque

Casos de uso principais:

- consultar saldo;
- registrar entrada de estoque;
- ajustar estoque;
- validar disponibilidade para venda;
- deduzir estoque apos venda confirmada.

### Caixa

Responsavel por abertura, operacao e fechamento de caixa.

Entidades principais:

- Caixa
- TurnoCaixa
- FechamentoCaixa

Casos de uso principais:

- abrir caixa;
- consultar caixa aberto;
- vincular venda ao caixa;
- fechar caixa;
- calcular total por forma de pagamento;
- registrar diferenca de caixa.

### Vendas

Responsavel pelo fluxo central do PDV.

Entidades principais:

- Venda
- ItemVenda

Casos de uso principais:

- iniciar venda;
- adicionar item;
- remover item;
- calcular total;
- finalizar venda;
- consultar venda;
- publicar evento de venda concluida.

### Pagamentos

Responsavel pelo registro da forma de pagamento e preparacao para integracoes futuras.

Entidades principais:

- Pagamento
- FormaPagamento
- Troco

Casos de uso principais:

- registrar pagamento;
- validar valor pago;
- calcular troco;
- consultar pagamento por venda.

### Relatorios e Auditoria

Responsavel por consultas consolidadas, historico e rastreabilidade.

Entidades principais:

- RegistroAuditoria
- RelatorioVenda
- RelatorioCaixa
- ProjecaoFinanceira

Casos de uso principais:

- consultar vendas por periodo;
- consultar vendas por vendedor;
- consultar fechamento de caixa;
- consultar resumo financeiro;
- registrar eventos relevantes para auditoria.

## Micro-servicos Propostos

A divisao abaixo evita criar um micro-servico por tabela. Cada servico representa um contexto de negocio.

### Servico de Identidade

Responsabilidade:

- usuarios;
- login;
- perfis;
- permissoes;
- emissao, renovacao e revogacao logica de JWT.

Banco proprio:

- usuarios;
- perfis;
- credenciais;
- refresh_tokens;
- auditoria_autenticacao.

Endpoints iniciais:

- `POST /auth/login`
- `POST /auth/refresh`
- `POST /auth/logout`
- `GET /auth/me`
- `POST /users`
- `GET /users`
- `GET /users/{id}`
- `PATCH /users/{id}/disable`

Eventos publicados:

- `identidade.usuario-criado.v1`
- `identidade.usuario-inativado.v1`

### Servico de Catalogo de Produtos

Responsabilidade:

- cadastro de produtos;
- consulta por ID;
- consulta por codigo de barras;
- preco de venda atual;
- status ativo/inativo.

Banco proprio:

- produtos;
- historico_precos, quando houver necessidade.

Endpoints iniciais:

- `POST /products`
- `GET /products`
- `GET /products/{id}`
- `GET /products/by-barcode/{ean}`
- `PUT /products/{id}`
- `PATCH /products/{id}/disable`

Eventos publicados:

- `catalogo.produto-criado.v1`
- `catalogo.produto-atualizado.v1`
- `catalogo.produto-inativado.v1`

### Servico de Estoque

Responsabilidade:

- saldo de estoque;
- movimentos de estoque;
- validacao de disponibilidade;
- deducao por venda confirmada.

Banco proprio:

- saldos_estoque;
- movimentos_estoque;
- reservas_estoque, se houver reserva temporaria.

Endpoints iniciais:

- `GET /inventory/products/{productId}`
- `POST /inventory/products/{productId}/entries`
- `POST /inventory/products/{productId}/adjustments`
- `POST /inventory/validate`
- `POST /inventory/deductions`

Eventos consumidos:

- `catalogo.produto-criado.v1`
- `vendas.venda-concluida.v1`

Eventos publicados:

- `estoque.saldo-criado.v1`
- `estoque.ajustado.v1`
- `estoque.deduzido.v1`
- `estoque.deducao-falhou.v1`

### Servico de Caixa

Responsabilidade:

- abertura de caixa;
- associacao de vendas ao caixa;
- fechamento de caixa;
- apuracao de valores por forma de pagamento;
- diferencas de caixa.

Banco proprio:

- caixas;
- turnos_caixa;
- fechamentos_caixa;
- vendas_caixa, como projecao local.

Endpoints iniciais:

- `POST /cashiers/open`
- `GET /cashiers/current`
- `POST /cashiers/{id}/close`
- `GET /cashiers/{id}/summary`

Eventos consumidos:

- `vendas.venda-concluida.v1`
- `pagamentos.pagamento-registrado.v1`

Eventos publicados:

- `caixa.aberto.v1`
- `caixa.fechado.v1`
- `caixa.diferenca-detectada.v1`

### Servico de Vendas

Responsabilidade:

- criacao da venda;
- itens da venda;
- calculo de subtotal e total;
- orquestracao da finalizacao;
- publicacao de venda concluida.

Banco proprio:

- vendas;
- itens_venda.

Endpoints iniciais:

- `POST /sales`
- `POST /sales/{id}/items`
- `DELETE /sales/{id}/items/{itemId}`
- `POST /sales/{id}/checkout`
- `GET /sales/{id}`
- `GET /sales?cashierId={cashierId}`

Chamadas sincronas:

- consulta produto no `Servico de Catalogo de Produtos`;
- valida estoque no `Servico de Estoque`;
- valida caixa aberto no `Servico de Caixa`;
- registra pagamento no `Servico de Pagamentos`.

Eventos publicados:

- `vendas.venda-iniciada.v1`
- `vendas.venda-concluida.v1`
- `vendas.venda-cancelada.v1`

### Servico de Pagamentos

Responsabilidade:

- registro da forma de pagamento;
- validacao de valor pago;
- calculo de troco;
- preparacao para integracoes futuras com Pix, cartao, TEF ou adquirentes.

Banco proprio:

- pagamentos;
- tentativas_pagamento, quando houver integracao externa.

Endpoints iniciais:

- `POST /payments`
- `GET /payments/{id}`
- `GET /payments/by-sale/{saleId}`

Eventos publicados:

- `pagamentos.pagamento-registrado.v1`
- `pagamentos.pagamento-rejeitado.v1`

### Servico de Relatorios

Responsabilidade:

- relatorios financeiros;
- historico de vendas;
- consultas por vendedor;
- consultas por caixa;
- leitura consolidada sem sobrecarregar o fluxo operacional.

Banco proprio:

- projecoes_vendas;
- projecoes_caixa;
- projecoes_financeiras.

Endpoints iniciais:

- `GET /reports/sales`
- `GET /reports/sales/by-seller`
- `GET /reports/cashiers/{cashierId}`
- `GET /reports/financial-summary`

Eventos consumidos:

- `vendas.venda-concluida.v1`
- `pagamentos.pagamento-registrado.v1`
- `caixa.aberto.v1`
- `caixa.fechado.v1`
- `estoque.deduzido.v1`

## Fluxos entre Micro-servicos

### Fluxo 1: Login

1. Cliente envia credenciais para `Servico de Identidade`.
2. `Servico de Identidade` valida login/e-mail e senha.
3. `Servico de Identidade` valida se o usuario esta ativo.
4. `Servico de Identidade` emite access token JWT com ID do usuario, perfil e claims minimas.
5. `Servico de Identidade` emite refresh token persistido com hash.
6. Cliente envia o access token no header `Authorization: Bearer`.
7. Demais servicos validam assinatura, issuer, audience, expiracao e perfil do token nas chamadas HTTP.

### Fluxo 1.1: Renovacao de Token

1. Cliente envia refresh token para `Servico de Identidade`.
2. `Servico de Identidade` valida hash, expiracao, usuario ativo e ausencia de revogacao.
3. `Servico de Identidade` revoga o refresh token antigo quando houver rotacao.
4. `Servico de Identidade` emite novo access token e novo refresh token.
5. Cliente passa a usar o novo access token nas chamadas protegidas.

### Fluxo 2: Cadastro de Produto

1. Usuario `ADM` chama `Servico de Catalogo de Produtos`.
2. `Servico de Catalogo de Produtos` valida EAN unico, descricao e precos.
3. Produto e salvo como ativo.
4. `Servico de Catalogo de Produtos` publica `catalogo.produto-criado.v1`.
5. `Servico de Estoque` consome o evento e cria saldo inicial, quando houver quantidade informada.

### Fluxo 3: Abertura de Caixa

1. Usuario `VENDEDOR` chama `Servico de Caixa`.
2. `Servico de Caixa` verifica se ja existe caixa aberto para o vendedor.
3. `Servico de Caixa` registra valor inicial, data/hora e vendedor.
4. `Servico de Caixa` publica `caixa.aberto.v1`.

### Fluxo 4: Venda no PDV

1. Usuario `VENDEDOR` inicia venda no `Servico de Vendas`.
2. `Servico de Vendas` valida se existe caixa aberto no `Servico de Caixa`.
3. Vendedor informa produto por ID ou EAN.
4. `Servico de Vendas` consulta `Servico de Catalogo de Produtos` para obter produto ativo e preco atual.
5. `Servico de Vendas` consulta `Servico de Estoque` para validar disponibilidade.
6. `Servico de Vendas` adiciona item com preco unitario congelado.
7. No checkout, `Servico de Vendas` recebe forma de pagamento.
8. `Servico de Vendas` chama `Servico de Pagamentos` para registrar pagamento.
9. `Servico de Vendas` solicita deducao ao `Servico de Estoque`.
10. Se pagamento e estoque forem confirmados, `Servico de Vendas` confirma a venda.
11. `Servico de Vendas` publica `vendas.venda-concluida.v1`.
12. `Servico de Caixa` e `Servico de Relatorios` consomem o evento para atualizar suas projecoes.

### Fluxo 5: Fechamento de Caixa

1. Usuario `VENDEDOR` solicita fechamento ao `Servico de Caixa`.
2. `Servico de Caixa` consulta sua projecao local de vendas do caixa.
3. `Servico de Caixa` calcula totais por forma de pagamento.
4. Vendedor informa o valor fisico em dinheiro.
5. `Servico de Caixa` calcula diferenca entre esperado e informado.
6. `Servico de Caixa` registra fechamento.
7. `Servico de Caixa` publica `caixa.fechado.v1`.
8. `Servico de Relatorios` consome o evento e atualiza relatorios.

## Consistencia e Mensageria

- Cada micro-servico deve gravar seus dados no proprio PostgreSQL antes de publicar eventos.
- No ambiente local, cada contexto deve usar um banco PostgreSQL proprio dentro do mesmo container Docker.
- Bancos locais definidos: `frente_caixa_identidade`, `frente_caixa_catalogo`, `frente_caixa_estoque`, `frente_caixa_caixa`, `frente_caixa_vendas`, `frente_caixa_pagamentos` e `frente_caixa_relatorios`.
- O RabbitMQ local deve usar o exchange de topico `frente-caixa.eventos`.
- Publicacao de eventos deve usar Outbox para evitar perda de mensagens.
- Consumo de eventos deve ser idempotente.
- Inbox pode ser usado para impedir processamento duplicado quando o consumidor tiver efeito persistente.
- Eventos devem ser nomeados por contexto, fato e versao, por exemplo `vendas.venda-concluida.v1`.
- Eventos representam fatos confirmados, nao comandos.
- Operacoes criticas do PDV devem retornar erro claro quando um servico dependente estiver indisponivel.

## Seguranca

- Senhas devem ser armazenadas com hash forte.
- Segredos JWT nao devem ficar no repositorio.
- Access tokens devem ter vida curta.
- Refresh tokens devem ser armazenados com hash e poder ser revogados.
- HTTPS deve ser obrigatorio fora do ambiente local.
- Autenticacao deve executar antes de autorizacao no pipeline ASP.NET Core.
- Rotas protegidas devem declarar autorizacao no grupo de endpoints ou endpoint especifico.
- Falhas de autenticacao devem retornar `401 Unauthorized`.
- Falhas de autorizacao devem retornar `403 Forbidden`.

## Dados Principais

### Usuario

- id
- nome
- login_email
- senha_hash
- perfil
- ativo
- criado_em
- atualizado_em

### RefreshToken

- id
- usuario_id
- token_hash
- criado_em
- expira_em
- revogado_em
- substituido_por_token_id
- endereco_ip_criacao
- endereco_ip_revogacao

### Produto

- id
- codigo_barras_ean
- descricao
- preco_custo
- preco_venda
- ativo
- criado_em
- atualizado_em

### Estoque

- id
- produto_id
- quantidade_disponivel
- atualizado_em

### MovimentoEstoque

- id
- produto_id
- tipo
- quantidade
- motivo
- referencia
- criado_em

### Caixa

- id
- vendedor_id
- aberto_em
- fechado_em
- valor_inicial
- valor_final_informado
- valor_esperado_dinheiro
- diferenca
- status

### Venda

- id
- caixa_id
- vendedor_id
- data_hora
- valor_total
- status

### ItemVenda

- id
- venda_id
- produto_id
- descricao_produto
- quantidade
- preco_unitario
- subtotal

### Pagamento

- id
- venda_id
- forma_pagamento
- valor
- troco
- status
- criado_em

## Estrutura Recomendada de Repositorio

Estrutura sugerida para manter micro-servicos com Clean Architecture:

```text
services/
  Identidade/
    src/
      FrenteCaixa.Identidade.Api/
      FrenteCaixa.Identidade.Application/
      FrenteCaixa.Identidade.Domain/
      FrenteCaixa.Identidade.Infrastructure/
    tests/
      FrenteCaixa.Identidade.Tests/
  CatalogoProdutos/
    src/
      FrenteCaixa.CatalogoProdutos.Api/
      FrenteCaixa.CatalogoProdutos.Application/
      FrenteCaixa.CatalogoProdutos.Domain/
      FrenteCaixa.CatalogoProdutos.Infrastructure/
    tests/
      FrenteCaixa.CatalogoProdutos.Tests/
  Estoque/
  Caixa/
  Vendas/
  Pagamentos/
  Relatorios/
docker/
docker-compose.yml
.spec/
STACK.md
architecture.md
```

## Primeira Sequencia de Implementacao

1. Criar `Servico de Identidade` com login, refresh token, perfis e JWT.
2. Criar `Servico de Catalogo de Produtos` com cadastro e consulta de produtos.
3. Criar `Servico de Estoque` com saldo inicial e movimentos.
4. Criar `Servico de Caixa` com abertura de caixa.
5. Criar `Servico de Vendas` com venda e itens.
6. Criar `Servico de Pagamentos` com pagamento simples.
7. Integrar venda, pagamento e estoque.
8. Adicionar RabbitMQ com Outbox para `vendas.venda-concluida.v1`.
9. Criar `Servico de Relatorios` consumindo eventos.

## Fora de Escopo Agora

- Front-end.
- Aplicativo desktop.
- Integracao real com TEF, Pix, bancos ou adquirentes.
- Cancelamento/devolucao de venda.
- Descontos, promocoes e cupons.
- Controle fiscal/NFC-e.
- Deploy em cloud.
- Observabilidade avancada.

## Decisoes Pendentes

- Lista final de micro-servicos para a primeira entrega.
- Estrategia de API Gateway.
- Padrao final de versionamento HTTP.
- Formato final de erros e codigos de negocio.
- Momento de incluir integracao real com pagamentos externos.
- Duracao exata do access token e refresh token.
- Estrategia final para credenciais de servico entre micro-servicos.
