---
name: "Frontend PDV React"
tags: [frontend, pdv, react]
---

# Intent

Criar o primeiro frontend web do FrenteCaixa para o operador de PDV usando React com JSX/HTML e CSS puro. O recorte deve permitir autenticar o operador, carregar o bootstrap do BFF e operar uma venda em andamento com inclusao e remocao de itens.

# Scope

- Criar app React em `frontend/pdv`.
- Usar Vite como servidor de desenvolvimento e build.
- Configurar proxy local para Identidade, BFF e Vendas.
- Implementar login via `POST /auth/login`.
- Carregar contexto do PDV via `GET /pdv/bootstrap`.
- Iniciar venda via `POST /sales`.
- Adicionar item via `POST /sales/{id}/items`.
- Remover item via `DELETE /sales/{id}/items/{itemId}`.

# Constraints

- Nao implementar checkout/finalizacao da venda neste recorte.
- Nao alterar contratos dos micro-servicos.
- Nao acessar bancos diretamente pelo frontend.
- Nao adicionar framework de UI pesado.
- O frontend deve conversar com APIs HTTP ja existentes.

# Out Of Scope

- Pagamento.
- Baixa de estoque.
- Impressao de cupom.
- Cadastro administrativo de produtos ou usuarios.
- Deploy em producao.

# Acceptance Criteria

Scenario: Operador autentica no PDV
  Given o servico de Identidade esta disponivel
  When o operador informa email e senha validos
  Then a tela armazena o access token localmente
  And carrega o bootstrap do BFF autenticado

Scenario: Bootstrap do PDV e exibido
  Given o operador esta autenticado
  When a tela chama o BFF
  Then exibe usuario, configuracao e status dos servicos

Scenario: Venda em andamento recebe itens
  Given o operador esta autenticado
  And informa um CaixaId valido
  When inicia uma venda
  And adiciona um item com produto, quantidade e preco
  Then a tela exibe o item e o total atualizado

Scenario: Item pode ser removido
  Given existe uma venda em andamento com item
  When o operador remove o item
  Then a tela atualiza a lista e o total

Scenario: Checkout ainda nao esta disponivel
  Given existe uma venda em andamento
  Then a tela nao permite finalizar pagamento neste recorte
