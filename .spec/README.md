# .spec - FrenteCaixa

Este diretorio guarda a documentacao de contrato do projeto.

Fluxo de trabalho recomendado:

1. `Discuss feature`: alinhar objetivo, regras e limites.
2. `Spec`: criar ou atualizar um contrato em `.spec`.
3. `Implement task`: implementar apenas o que o contrato permite.
4. `Verify`: rodar build, testes e validacoes manuais quando necessario.

Arquivos iniciais:

- `project.spec.md`: contrato geral do projeto.
- `tasks/bootstrap-dotnet-backend.spec.md`: primeira tarefa recomendada para criar a base do back-end.

Quando uma feature nova for aprovada, crie um arquivo em `.spec/tasks/` com:

- objetivo;
- decisoes ja confirmadas;
- limites do que pode mudar;
- criterios de aceite com cenarios testaveis;
- itens fora de escopo.

Observacao: o CLI `agent-spec` nao foi encontrado no PATH desta maquina durante a criacao inicial destes arquivos. A estrutura foi escrita no formato esperado, mas a validacao formal por `agent-spec parse` e `agent-spec lint` fica pendente ate o CLI estar instalado.
