# FrenteCaixa PDV

Frontend web do operador de PDV, criado com React, Vite, JSX e CSS puro.

## Scripts

```powershell
npm install
npm run dev
npm run lint
npm run build
```

## Desenvolvimento local

O Vite sobe por padrao em `http://127.0.0.1:5173/`.

As chamadas HTTP usam proxy local:

- `/identity` -> `http://localhost:5227`
- `/bff` -> `http://localhost:5265`
- `/vendas` -> `http://localhost:5165`

O checkout permanece fora deste recorte; a tela opera somente venda em andamento.
