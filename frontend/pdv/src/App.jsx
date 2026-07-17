import { useEffect, useMemo, useState } from 'react'
import {
  AlertCircle,
  BarChart3,
  Ban,
  CircleDollarSign,
  LogIn,
  LogOut,
  Package,
  Pencil,
  Plus,
  RefreshCw,
  ReceiptText,
  Save,
  Search,
  ShoppingCart,
  Trash2,
  User,
} from 'lucide-react'
import './App.css'

const AUTH_KEY = 'frente-caixa-pdv-auth'
const emptyGuid = '00000000-0000-0000-0000-000000000000'

const initialLogin = {
  email: 'vendedor@frentecaixa.local',
  senha: 'Senha@123',
}

const initialCashForm = {
  valorInicial: '0',
}

const initialCloseCashForm = {
  valorFechamento: '',
}

const initialItemForm = {
  produtoId: '',
  descricaoProduto: '',
  quantidade: '1',
  precoUnitario: '',
}

const initialProductSearch = {
  termo: '',
  codigoBarras: '',
}

const initialCheckoutForm = {
  formaPagamento: 'Dinheiro',
  valorPago: '',
}

const initialAdminReports = {
  sales: [],
  summary: null,
}

const initialAdminProductSearch = {
  termo: '',
  somenteAtivos: false,
}

const initialAdminProductForm = {
  id: '',
  descricao: '',
  codigoBarrasEan: '',
  precoCusto: '',
  precoVenda: '',
}

const paymentMethods = ['Dinheiro', 'Cartao', 'Pix']

function App() {
  const [auth, setAuth] = useState(readStoredAuth)
  const [loginForm, setLoginForm] = useState(initialLogin)
  const [cashForm, setCashForm] = useState(initialCashForm)
  const [closeCashForm, setCloseCashForm] = useState(initialCloseCashForm)
  const [itemForm, setItemForm] = useState(initialItemForm)
  const [productSearch, setProductSearch] = useState(initialProductSearch)
  const [checkoutForm, setCheckoutForm] = useState(initialCheckoutForm)
  const [productResults, setProductResults] = useState([])
  const [selectedProduct, setSelectedProduct] = useState(null)
  const [checkoutResult, setCheckoutResult] = useState(null)
  const [bootstrap, setBootstrap] = useState(null)
  const [caixaAtual, setCaixaAtual] = useState(null)
  const [movimentacoesCaixa, setMovimentacoesCaixa] = useState([])
  const [resumoCaixa, setResumoCaixa] = useState(null)
  const [venda, setVenda] = useState(null)
  const [activeView, setActiveView] = useState('vendas')
  const [adminReports, setAdminReports] = useState(initialAdminReports)
  const [adminProducts, setAdminProducts] = useState([])
  const [adminProductSearch, setAdminProductSearch] = useState(initialAdminProductSearch)
  const [adminProductForm, setAdminProductForm] = useState(initialAdminProductForm)
  const [loading, setLoading] = useState('')
  const [error, setError] = useState('')

  const isAuthenticated = Boolean(auth?.accessToken)
  const operatorProfile = bootstrap?.usuario?.perfil ?? auth?.usuario?.perfil ?? ''
  const isAdmin = operatorProfile === 'ADM'
  const currency = bootstrap?.configuracao?.moeda ?? 'BRL'
  const casasDecimais = bootstrap?.configuracao?.casasDecimais ?? 2
  const caixaAberto = caixaAtual?.status === 'Aberto'
  const vendaEmAndamento = venda?.status === 'EmAndamento'
  const podeEditarVenda = Boolean(venda?.id && vendaEmAndamento)
  const podeFinalizarVenda = Boolean(podeEditarVenda && venda?.itens?.length)
  const valorPagoCheckout =
    checkoutForm.formaPagamento === 'Dinheiro'
      ? Number(checkoutForm.valorPago)
      : Number(venda?.total ?? 0)
  const trocoPrevisto = checkoutForm.formaPagamento === 'Dinheiro'
    ? Math.max(0, valorPagoCheckout - Number(venda?.total ?? 0))
    : 0
  const valorFechamentoInformado = closeCashForm.valorFechamento === ''
    ? null
    : Number(closeCashForm.valorFechamento)
  const diferencaFechamentoPrevista =
    valorFechamentoInformado === null || !resumoCaixa
      ? null
      : valorFechamentoInformado - Number(resumoCaixa.dinheiroEsperado)

  const totalItens = useMemo(() => {
    return venda?.itens?.reduce((sum, item) => sum + Number(item.quantidade), 0) ?? 0
  }, [venda])

  useEffect(() => {
    if (!isAuthenticated) {
      setBootstrap(null)
      setCaixaAtual(null)
      setMovimentacoesCaixa([])
      setResumoCaixa(null)
      setVenda(null)
      setCheckoutResult(null)
      setProductResults([])
      setSelectedProduct(null)
      setActiveView('vendas')
      setAdminReports(initialAdminReports)
      setAdminProducts([])
      setAdminProductSearch(initialAdminProductSearch)
      setAdminProductForm(initialAdminProductForm)
      return
    }

    loadPdvContext(auth.accessToken)
  }, [auth?.accessToken, isAuthenticated])

  useEffect(() => {
    if (!isAdmin && (activeView === 'relatorios' || activeView === 'produtos')) {
      setActiveView('vendas')
    }
  }, [activeView, isAdmin])

  async function login(event) {
    event.preventDefault()
    setError('')
    setLoading('login')

    try {
      const response = await requestJson('/identity/auth/login', {
        method: 'POST',
        body: loginForm,
      })
      setStoredAuth(response)
      setAuth(response)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function loadPdvContext(token = auth?.accessToken) {
    if (!token) {
      return
    }

    setError('')
    setLoading('contexto')

    try {
      const [bootstrapResponse, caixaResponse] = await Promise.all([
        requestJson('/bff/pdv/bootstrap', { token }),
        requestJson('/bff/pdv/cash-register/current', { token, notFoundAsNull: true }),
      ])
      setBootstrap(bootstrapResponse)
      setCaixaAtual(caixaResponse)
      if (caixaResponse?.status === 'Aberto') {
        const [movimentacoes, resumo] = await Promise.all([
          requestJson('/bff/pdv/cash-register/current/movements', { token }),
          requestJson('/bff/pdv/cash-register/current/summary', { token }),
        ])
        setMovimentacoesCaixa(movimentacoes)
        setResumoCaixa(resumo)
      } else {
        setMovimentacoesCaixa([])
        setResumoCaixa(null)
      }
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function abrirCaixa(event) {
    event.preventDefault()
    setError('')
    setLoading('abrir-caixa')

    try {
      const caixa = await requestJson('/bff/pdv/cash-register/open', {
        method: 'POST',
        token: auth.accessToken,
        body: {
          valorInicial: Number(cashForm.valorInicial),
        },
      })
      setCaixaAtual(caixa)
      const movimentacoes = await requestJson('/bff/pdv/cash-register/current/movements', {
        token: auth.accessToken,
      })
      const resumo = await requestJson('/bff/pdv/cash-register/current/summary', {
        token: auth.accessToken,
      })
      setMovimentacoesCaixa(movimentacoes)
      setResumoCaixa(resumo)
      setCashForm(initialCashForm)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function iniciarVenda(event) {
    event.preventDefault()

    if (!caixaAberto) {
      setError('Abra um caixa antes de iniciar a venda.')
      return
    }

    setError('')
    setLoading('venda')

    try {
      const response = await requestJson('/vendas/sales', {
        method: 'POST',
        token: auth.accessToken,
        body: {
          caixaId: caixaAtual.id,
        },
      })
      setVenda(response)
      setCheckoutResult(null)
      setCheckoutForm(initialCheckoutForm)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function adicionarItem(event) {
    event.preventDefault()

    if (!venda?.id) {
      setError('Inicie uma venda antes de adicionar itens.')
      return
    }

    if (!vendaEmAndamento) {
      setError('Venda concluida nao permite novos itens.')
      return
    }

    setError('')
    setLoading('item')

    try {
      const response = await requestJson(`/vendas/sales/${venda.id}/items`, {
        method: 'POST',
        token: auth.accessToken,
        body: {
          produtoId: itemForm.produtoId,
          descricaoProduto: itemForm.descricaoProduto,
          quantidade: Number(itemForm.quantidade),
          precoUnitario: Number(itemForm.precoUnitario),
        },
      })
      setVenda(response)
      setItemForm(initialItemForm)
      setSelectedProduct(null)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function removerItem(itemId) {
    if (!vendaEmAndamento) {
      setError('Venda concluida nao permite remover itens.')
      return
    }

    setError('')
    setLoading(`remover-${itemId}`)

    try {
      const response = await requestJson(`/vendas/sales/${venda.id}/items/${itemId}`, {
        method: 'DELETE',
        token: auth.accessToken,
      })
      setVenda(response)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  function logout() {
    sessionStorage.removeItem(AUTH_KEY)
    setAuth(null)
    setLoginForm(initialLogin)
    setCashForm(initialCashForm)
    setCloseCashForm(initialCloseCashForm)
    setItemForm(initialItemForm)
    setProductSearch(initialProductSearch)
    setCheckoutForm(initialCheckoutForm)
    setProductResults([])
    setSelectedProduct(null)
    setCheckoutResult(null)
    setCaixaAtual(null)
    setMovimentacoesCaixa([])
    setResumoCaixa(null)
    setBootstrap(null)
    setError('')
    setActiveView('vendas')
    setAdminReports(initialAdminReports)
    setAdminProducts([])
    setAdminProductSearch(initialAdminProductSearch)
    setAdminProductForm(initialAdminProductForm)
  }

  async function fecharCaixa(event) {
    event.preventDefault()

    if (!caixaAtual?.id || !caixaAberto) {
      setError('Nao ha caixa aberto para fechar.')
      return
    }

    const valorFechamento = Number(closeCashForm.valorFechamento)

    if (valorFechamento < 0) {
      setError('Valor de fechamento nao pode ser negativo.')
      return
    }

    setError('')
    setLoading('fechar-caixa')

    try {
      const caixa = await requestJson(`/bff/pdv/cash-register/${caixaAtual.id}/close`, {
        method: 'POST',
        token: auth.accessToken,
        body: {
          valorFechamento,
        },
      })
      setCaixaAtual(caixa)
      setMovimentacoesCaixa([])
      setResumoCaixa(null)
      setCloseCashForm(initialCloseCashForm)
      setVenda(null)
      setCheckoutResult(null)
      setItemForm(initialItemForm)
      setSelectedProduct(null)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function finalizarVenda(event) {
    event.preventDefault()

    if (!podeFinalizarVenda) {
      setError('Inicie uma venda com itens antes do checkout.')
      return
    }

    if (checkoutForm.formaPagamento === 'Dinheiro' && valorPagoCheckout < Number(venda.total)) {
      setError('Valor recebido em dinheiro deve ser maior ou igual ao total.')
      return
    }

    setError('')
    setLoading('checkout')

    try {
      const checkout = await requestJson(`/vendas/sales/${venda.id}/checkout`, {
        method: 'POST',
        token: auth.accessToken,
        body: {
          formaPagamento: checkoutForm.formaPagamento,
          valorPago: valorPagoCheckout,
        },
      })
      setVenda(checkout.venda)
      setCheckoutResult({
        pagamentoId: checkout.pagamentoId,
        concluidaEm: checkout.concluidaEm,
        formaPagamento: checkoutForm.formaPagamento,
        valorPago: valorPagoCheckout,
        troco: trocoPrevisto,
      })
      const resumo = await requestJson('/bff/pdv/cash-register/current/summary', {
        token: auth.accessToken,
      })
      setResumoCaixa(resumo)
      setSelectedProduct(null)
      setItemForm(initialItemForm)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function buscarProdutos(event) {
    event.preventDefault()
    setError('')
    setLoading('produtos')

    try {
      const params = new URLSearchParams()

      if (productSearch.termo.trim()) {
        params.set('term', productSearch.termo.trim())
      }

      const response = await requestJson(`/bff/pdv/products?${params.toString()}`, {
        token: auth.accessToken,
      })
      setProductResults(response)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function buscarProdutoPorCodigoBarras(event) {
    event.preventDefault()
    const codigoBarras = productSearch.codigoBarras.trim()

    if (!codigoBarras) {
      setError('Informe o codigo de barras para buscar.')
      return
    }

    setError('')
    setLoading('barcode')

    try {
      const produto = await requestJson(`/bff/pdv/products/by-barcode/${encodeURIComponent(codigoBarras)}`, {
        token: auth.accessToken,
      })
      selecionarProduto(produto)
      setProductResults([produto])
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  function selecionarProduto(produto) {
    setSelectedProduct(produto)
    setItemForm((current) => ({
      ...current,
      produtoId: produto.id,
      descricaoProduto: produto.descricao,
      precoUnitario: String(produto.precoVenda),
    }))
  }

  async function carregarRelatoriosAdmin() {
    if (!isAdmin) {
      return
    }

    setError('')
    setLoading('relatorios')

    try {
      const [sales, summary] = await Promise.all([
        requestJson('/bff/admin/reports/sales', { token: auth.accessToken }),
        requestJson('/bff/admin/reports/financial-summary', { token: auth.accessToken }),
      ])

      setAdminReports({ sales, summary })
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  function abrirViewRelatorios() {
    setActiveView('relatorios')

    if (!adminReports.summary && loading !== 'relatorios') {
      carregarRelatoriosAdmin()
    }
  }

  async function carregarProdutosAdmin(event) {
    event?.preventDefault()

    if (!isAdmin) {
      return
    }

    setError('')
    setLoading('admin-produtos')

    try {
      const params = new URLSearchParams()

      if (adminProductSearch.termo.trim()) {
        params.set('term', adminProductSearch.termo.trim())
      }

      params.set('onlyActive', String(adminProductSearch.somenteAtivos))

      const produtos = await requestJson(`/bff/admin/products?${params.toString()}`, {
        token: auth.accessToken,
      })
      setAdminProducts(produtos)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  function abrirViewProdutos() {
    setActiveView('produtos')

    if (!adminProducts.length && loading !== 'admin-produtos') {
      carregarProdutosAdmin()
    }
  }

  function selecionarProdutoAdmin(produto) {
    setAdminProductForm({
      id: produto.id,
      descricao: produto.descricao,
      codigoBarrasEan: produto.codigoBarrasEan ?? '',
      precoCusto: String(produto.precoCusto),
      precoVenda: String(produto.precoVenda),
    })
  }

  function limparFormularioProdutoAdmin() {
    setAdminProductForm(initialAdminProductForm)
  }

  async function salvarProdutoAdmin(event) {
    event.preventDefault()

    const precoCusto = Number(adminProductForm.precoCusto)
    const precoVenda = Number(adminProductForm.precoVenda)

    if (!adminProductForm.descricao.trim()) {
      setError('Informe a descricao do produto.')
      return
    }

    if (precoCusto < 0 || precoVenda <= 0) {
      setError('Preco de custo nao pode ser negativo e preco de venda deve ser maior que zero.')
      return
    }

    setError('')
    setLoading('salvar-produto')

    try {
      const editando = Boolean(adminProductForm.id)
      const produto = await requestJson(
        editando
          ? `/bff/admin/products/${adminProductForm.id}`
          : '/bff/admin/products',
        {
          method: editando ? 'PUT' : 'POST',
          token: auth.accessToken,
          body: {
            descricao: adminProductForm.descricao.trim(),
            codigoBarrasEan: adminProductForm.codigoBarrasEan.trim() || null,
            precoCusto,
            precoVenda,
          },
        },
      )

      setAdminProducts((current) => {
        const existing = current.some((item) => item.id === produto.id)

        return existing
          ? current.map((item) => (item.id === produto.id ? produto : item))
          : [produto, ...current]
      })
      setProductResults((current) =>
        current.map((item) =>
          item.id === produto.id
            ? {
              ...item,
              descricao: produto.descricao,
              codigoBarrasEan: produto.codigoBarrasEan,
              precoVenda: produto.precoVenda,
              ativo: produto.ativo,
            }
            : item,
        ),
      )
      limparFormularioProdutoAdmin()
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function inativarProdutoAdmin(produto) {
    setError('')
    setLoading(`inativar-produto-${produto.id}`)

    try {
      await requestJson(`/bff/admin/products/${produto.id}/disable`, {
        method: 'PATCH',
        token: auth.accessToken,
      })
      const produtoInativo = { ...produto, ativo: false }
      setAdminProducts((current) =>
        current.map((item) => (item.id === produto.id ? produtoInativo : item)),
      )
      setProductResults((current) =>
        current.filter((item) => item.id !== produto.id),
      )
      if (adminProductForm.id === produto.id) {
        limparFormularioProdutoAdmin()
      }
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  if (!isAuthenticated) {
    return (
      <main className="login-shell">
        <section className="login-panel" aria-labelledby="login-title">
          <div className="brand-mark">
            <CircleDollarSign aria-hidden="true" size={28} />
          </div>
          <p className="eyebrow">Cantina MCV</p>
          <h1 id="login-title">PDV</h1>

          <form className="login-form" onSubmit={login}>
            <label>
              Email
              <input
                autoComplete="username"
                type="email"
                value={loginForm.email}
                onChange={(event) =>
                  setLoginForm((current) => ({ ...current, email: event.target.value }))
                }
                required
              />
            </label>
            <label>
              Senha
              <input
                autoComplete="current-password"
                type="password"
                value={loginForm.senha}
                onChange={(event) =>
                  setLoginForm((current) => ({ ...current, senha: event.target.value }))
                }
                required
              />
            </label>
            {error && <InlineError message={error} />}
            <button type="submit" className="primary-action" disabled={loading === 'login'}>
              <LogIn aria-hidden="true" size={18} />
              {loading === 'login' ? 'Entrando...' : 'Entrar'}
            </button>
          </form>
        </section>
      </main>
    )
  }

  return (
    <main className="app-shell">
      <header className="topbar">
        <div>
          <p className="eyebrow">Cantina MCV</p>
          <h1>PDV</h1>
        </div>
        <div className="operator-strip">
          <div className="operator-chip">
            <User aria-hidden="true" size={18} />
            <span>{bootstrap?.usuario?.nome ?? auth.usuario?.nome ?? 'Operador'}</span>
          </div>
          <button
            type="button"
            className="icon-action"
            onClick={() => loadPdvContext()}
            title="Atualizar contexto do PDV"
            aria-label="Atualizar contexto do PDV"
            disabled={loading === 'contexto'}
          >
            <RefreshCw aria-hidden="true" size={18} />
          </button>
          <button type="button" className="icon-action" onClick={logout} title="Sair" aria-label="Sair">
            <LogOut aria-hidden="true" size={18} />
          </button>
        </div>
      </header>

      <nav className="view-tabs" aria-label="Areas do sistema">
        <button
          type="button"
          className={activeView === 'vendas' ? 'view-tab active' : 'view-tab'}
          aria-pressed={activeView === 'vendas'}
          onClick={() => setActiveView('vendas')}
        >
          <ShoppingCart aria-hidden="true" size={17} />
          Vendas
        </button>
        {isAdmin && (
          <>
            <button
              type="button"
              className={activeView === 'produtos' ? 'view-tab active' : 'view-tab'}
              aria-pressed={activeView === 'produtos'}
              onClick={abrirViewProdutos}
            >
              <Package aria-hidden="true" size={17} />
              Produtos
            </button>
            <button
              type="button"
              className={activeView === 'relatorios' ? 'view-tab active' : 'view-tab'}
              aria-pressed={activeView === 'relatorios'}
              onClick={abrirViewRelatorios}
            >
              <BarChart3 aria-hidden="true" size={17} />
              Relatorios
            </button>
          </>
        )}
      </nav>

      {error && <InlineError message={error} />}

      <section className="status-band" aria-label="Status do PDV">
        <Metric label="Status" value={loading ? 'Sincronizando' : 'Operacional'} />
        <Metric label="Perfil" value={operatorProfile || '-'} />
        <Metric label="Caixa" value={caixaAtual?.status ?? 'Fechado'} />
        <Metric label="Itens" value={String(totalItens)} />
        <Metric label="Total" value={formatMoney(venda?.total ?? 0, currency, casasDecimais)} strong />
      </section>

      <section className="workspace">
        {activeView === 'produtos' && isAdmin ? (
          <AdminProductsView
            products={adminProducts}
            search={adminProductSearch}
            form={adminProductForm}
            loading={loading}
            currency={currency}
            casasDecimais={casasDecimais}
            onSearchChange={setAdminProductSearch}
            onFormChange={setAdminProductForm}
            onSubmitSearch={carregarProdutosAdmin}
            onSubmitForm={salvarProdutoAdmin}
            onSelectProduct={selecionarProdutoAdmin}
            onDisableProduct={inativarProdutoAdmin}
            onClearForm={limparFormularioProdutoAdmin}
          />
        ) : activeView === 'relatorios' && isAdmin ? (
          <AdminReportsView
            reports={adminReports}
            loading={loading}
            currency={currency}
            casasDecimais={casasDecimais}
            onRefresh={carregarRelatoriosAdmin}
          />
        ) : (
        <div className="sale-pane">
          <div className="section-heading">
            <ShoppingCart aria-hidden="true" size={20} />
            <h2>Venda</h2>
          </div>

          <section className="cash-panel" aria-label="Caixa operacional">
            {caixaAtual && (
              <div className="cash-summary">
                <span>
                  <strong>Caixa {caixaAtual.status}</strong>
                  <small>{caixaAtual.id}</small>
                </span>
                <em>
                  {formatMoney(
                    caixaAtual.valorFechamento ?? caixaAtual.valorInicial,
                    currency,
                    casasDecimais,
                  )}
                </em>
              </div>
            )}

            {!caixaAberto && (
              <form className="inline-form" onSubmit={abrirCaixa}>
                <label>
                  Valor inicial
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    value={cashForm.valorInicial}
                    onChange={(event) =>
                      setCashForm((current) => ({ ...current, valorInicial: event.target.value }))
                    }
                    required
                  />
                </label>
                <button
                  type="submit"
                  className="secondary-action"
                  disabled={loading === 'abrir-caixa'}
                >
                  <CircleDollarSign aria-hidden="true" size={16} />
                  {loading === 'abrir-caixa' ? 'Abrindo...' : 'Abrir caixa'}
                </button>
              </form>
            )}

            {caixaAberto && (
              <>
                {resumoCaixa && (
                  <div className="cash-turn-summary" aria-label="Resumo do turno">
                    <Metric
                      label="Vendas"
                      value={String(resumoCaixa.quantidadeVendas)}
                    />
                    <Metric
                      label="Total vendido"
                      value={formatMoney(resumoCaixa.totalVendido, currency, casasDecimais)}
                      strong
                    />
                    <Metric
                      label="Dinheiro esperado"
                      value={formatMoney(resumoCaixa.dinheiroEsperado, currency, casasDecimais)}
                    />
                    {diferencaFechamentoPrevista !== null && (
                      <Metric
                        label="Diferenca"
                        value={formatMoney(diferencaFechamentoPrevista, currency, casasDecimais)}
                      />
                    )}
                    {resumoCaixa.totaisPorFormaPagamento?.length > 0 && (
                      <div className="payment-totals">
                        {resumoCaixa.totaisPorFormaPagamento.map((total) => (
                          <span key={total.formaPagamento}>
                            <strong>{total.formaPagamento}</strong>
                            <em>{formatMoney(total.total, currency, casasDecimais)}</em>
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </>
            )}

            {caixaAberto && (
              <form className="inline-form close-cash-form" onSubmit={fecharCaixa}>
                <label>
                  Valor fechamento
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    value={closeCashForm.valorFechamento}
                    onChange={(event) =>
                      setCloseCashForm((current) => ({
                        ...current,
                        valorFechamento: event.target.value,
                      }))
                    }
                    required
                  />
                </label>
                <button
                  type="submit"
                  className="secondary-action"
                  disabled={loading === 'fechar-caixa'}
                >
                  <CircleDollarSign aria-hidden="true" size={16} />
                  {loading === 'fechar-caixa' ? 'Fechando...' : 'Fechar caixa'}
                </button>
              </form>
            )}

            {movimentacoesCaixa.length > 0 && (
              <div className="cash-movements" aria-label="Movimentacoes do caixa">
                {movimentacoesCaixa.slice(0, 4).map((movimentacao) => (
                  <div className="cash-movement-row" key={movimentacao.id}>
                    <span>{movimentacao.tipo}</span>
                    <strong>{formatMoney(movimentacao.valor, currency, casasDecimais)}</strong>
                  </div>
                ))}
              </div>
            )}

            <form className="inline-form start-sale-form" onSubmit={iniciarVenda}>
              <div className="form-note">
                <span>Operacao</span>
                <strong>{venda ? `Venda ${venda.status}` : 'Pronta para venda'}</strong>
              </div>
              <button
                type="submit"
                className="primary-action"
                disabled={!caixaAberto || loading === 'venda'}
              >
                <Plus aria-hidden="true" size={18} />
                {venda ? 'Nova venda' : 'Iniciar'}
              </button>
            </form>
          </section>

          <section className="product-finder" aria-labelledby="product-search-title">
            <div className="section-heading">
              <Search aria-hidden="true" size={20} />
              <h2 id="product-search-title">Produto</h2>
            </div>

            <form className="product-search-form" onSubmit={buscarProdutos}>
              <label>
                Termo
                <input
                  value={productSearch.termo}
                  onChange={(event) =>
                    setProductSearch((current) => ({ ...current, termo: event.target.value }))
                  }
                  placeholder="Cafe, acucar, 789..."
                />
              </label>
              <button type="submit" className="secondary-action" disabled={loading === 'produtos'}>
                <Search aria-hidden="true" size={16} />
                Buscar
              </button>
            </form>

            <form className="product-search-form barcode-form" onSubmit={buscarProdutoPorCodigoBarras}>
              <label>
                Codigo de barras
                <input
                  value={productSearch.codigoBarras}
                  onChange={(event) =>
                    setProductSearch((current) => ({ ...current, codigoBarras: event.target.value }))
                  }
                  placeholder="7891234567895"
                />
              </label>
              <button type="submit" className="secondary-action" disabled={loading === 'barcode'}>
                <Search aria-hidden="true" size={16} />
                Localizar
              </button>
            </form>

            {selectedProduct && (
              <div className="selected-product">
                <span>Selecionado</span>
                <strong>{selectedProduct.descricao}</strong>
                <em>{formatMoney(selectedProduct.precoVenda, currency, casasDecimais)}</em>
              </div>
            )}

            <div className="product-results" aria-live="polite">
              {productResults.map((produto) => (
                <button
                  type="button"
                  className="product-result-row"
                  key={produto.id}
                  onClick={() => selecionarProduto(produto)}
                >
                  <span>
                    <strong>{produto.descricao}</strong>
                    <small>{produto.codigoBarrasEan ?? 'Sem codigo de barras'}</small>
                  </span>
                  <em>{formatMoney(produto.precoVenda, currency, casasDecimais)}</em>
                </button>
              ))}
            </div>
          </section>

          <form className="item-form" onSubmit={adicionarItem}>
            <label className="span-2">
              ProdutoId
              <input
                value={itemForm.produtoId}
                onChange={(event) =>
                  setItemForm((current) => ({ ...current, produtoId: event.target.value }))
                }
                placeholder={emptyGuid}
                readOnly
                required
              />
            </label>
            <label className="span-2">
              Descricao
              <input
                value={itemForm.descricaoProduto}
                onChange={(event) =>
                  setItemForm((current) => ({ ...current, descricaoProduto: event.target.value }))
                }
                placeholder="Cafe 500g"
                readOnly
                required
              />
            </label>
            <label>
              Qtd
              <input
                type="number"
                min="0.01"
                step="0.01"
                value={itemForm.quantidade}
                onChange={(event) =>
                  setItemForm((current) => ({ ...current, quantidade: event.target.value }))
                }
                required
              />
            </label>
            <label>
              Preco
              <input
                type="number"
                min="0.01"
                step="0.01"
                value={itemForm.precoUnitario}
                onChange={(event) =>
                  setItemForm((current) => ({ ...current, precoUnitario: event.target.value }))
                }
                required
              />
            </label>
            <button
              type="submit"
              className="primary-action span-3"
              disabled={!podeEditarVenda || loading === 'item'}
            >
              <Plus aria-hidden="true" size={18} />
              Adicionar item
            </button>
          </form>

          <div className="items-table" role="table" aria-label="Itens da venda">
            <div className="items-row items-head" role="row">
              <span role="columnheader">Produto</span>
              <span role="columnheader">Qtd</span>
              <span role="columnheader">Preco</span>
              <span role="columnheader">Subtotal</span>
              <span role="columnheader">Acoes</span>
            </div>
            {venda?.itens?.length ? (
              venda.itens.map((item) => (
                <div className="items-row" role="row" key={item.id}>
                  <span role="cell">{item.descricaoProduto}</span>
                  <span role="cell">{formatNumber(item.quantidade)}</span>
                  <span role="cell">{formatMoney(item.precoUnitario, currency, casasDecimais)}</span>
                  <span role="cell">{formatMoney(item.subtotal, currency, casasDecimais)}</span>
                  <span role="cell">
                    <button
                      type="button"
                      className="icon-action danger"
                      onClick={() => removerItem(item.id)}
                      title="Remover item"
                      aria-label={`Remover ${item.descricaoProduto}`}
                      disabled={!podeEditarVenda || loading === `remover-${item.id}`}
                    >
                      <Trash2 aria-hidden="true" size={17} />
                    </button>
                  </span>
                </div>
              ))
            ) : (
              <div className="empty-state">Venda sem itens</div>
            )}
          </div>

          <form className="checkout-panel" onSubmit={finalizarVenda}>
            <div className="checkout-summary">
              <span>Total</span>
              <strong>{formatMoney(venda?.total ?? 0, currency, casasDecimais)}</strong>
            </div>

            <div className="payment-methods" role="group" aria-label="Forma de pagamento">
              {paymentMethods.map((formaPagamento) => (
                <button
                  type="button"
                  key={formaPagamento}
                  className={
                    checkoutForm.formaPagamento === formaPagamento
                      ? 'payment-mode active'
                      : 'payment-mode'
                  }
                  aria-pressed={checkoutForm.formaPagamento === formaPagamento}
                  onClick={() =>
                    setCheckoutForm((current) => ({
                      ...current,
                      formaPagamento,
                      valorPago:
                        formaPagamento === 'Dinheiro'
                          ? current.valorPago
                          : String(venda?.total ?? 0),
                    }))
                  }
                  disabled={!podeEditarVenda}
                >
                  {formaPagamento}
                </button>
              ))}
            </div>

            <label>
              Valor recebido
              <input
                type="number"
                min="0.01"
                step="0.01"
                value={
                  checkoutForm.formaPagamento === 'Dinheiro'
                    ? checkoutForm.valorPago
                    : String(venda?.total ?? 0)
                }
                onChange={(event) =>
                  setCheckoutForm((current) => ({ ...current, valorPago: event.target.value }))
                }
                readOnly={checkoutForm.formaPagamento !== 'Dinheiro'}
                required
              />
            </label>

            <div className="change-preview">
              <span>Troco</span>
              <strong>{formatMoney(trocoPrevisto, currency, casasDecimais)}</strong>
            </div>

            <button
              type="submit"
              className="primary-action checkout-action"
              disabled={!podeFinalizarVenda || loading === 'checkout'}
            >
              <CircleDollarSign aria-hidden="true" size={18} />
              {loading === 'checkout' ? 'Finalizando...' : 'Checkout'}
            </button>

            {checkoutResult && (
              <div className="checkout-result" role="status">
                <span>Venda concluida</span>
                <strong>{checkoutResult.formaPagamento}</strong>
                <small>Pagamento {checkoutResult.pagamentoId}</small>
                <em>Troco {formatMoney(checkoutResult.troco, currency, casasDecimais)}</em>
              </div>
            )}
          </form>
        </div>
        )}

      </section>
    </main>
  )
}

function AdminProductsView({
  products,
  search,
  form,
  loading,
  currency,
  casasDecimais,
  onSearchChange,
  onFormChange,
  onSubmitSearch,
  onSubmitForm,
  onSelectProduct,
  onDisableProduct,
  onClearForm,
}) {
  const editing = Boolean(form.id)

  return (
    <div className="admin-pane">
      <div className="admin-header">
        <div className="section-heading">
          <Package aria-hidden="true" size={20} />
          <h2>Produtos</h2>
        </div>
        <button
          type="button"
          className="secondary-action"
          onClick={onClearForm}
          disabled={!editing && !form.descricao && !form.codigoBarrasEan && !form.precoVenda}
        >
          <Plus aria-hidden="true" size={16} />
          Novo
        </button>
      </div>

      <form className="admin-product-search" onSubmit={onSubmitSearch}>
        <label>
          Busca
          <input
            value={search.termo}
            onChange={(event) =>
              onSearchChange((current) => ({ ...current, termo: event.target.value }))
            }
            placeholder="Cafe, bolo, 789..."
          />
        </label>
        <label className="checkbox-label">
          <input
            type="checkbox"
            checked={search.somenteAtivos}
            onChange={(event) =>
              onSearchChange((current) => ({ ...current, somenteAtivos: event.target.checked }))
            }
          />
          Somente ativos
        </label>
        <button type="submit" className="secondary-action" disabled={loading === 'admin-produtos'}>
          <Search aria-hidden="true" size={16} />
          {loading === 'admin-produtos' ? 'Buscando...' : 'Buscar'}
        </button>
      </form>

      <form className="admin-product-form" onSubmit={onSubmitForm}>
        <label className="span-2">
          Descricao
          <input
            value={form.descricao}
            onChange={(event) =>
              onFormChange((current) => ({ ...current, descricao: event.target.value }))
            }
            placeholder="Cafe Torrado 500g"
            required
          />
        </label>
        <label>
          Codigo de barras
          <input
            value={form.codigoBarrasEan}
            onChange={(event) =>
              onFormChange((current) => ({ ...current, codigoBarrasEan: event.target.value }))
            }
            placeholder="7891234567895"
          />
        </label>
        <label>
          Preco custo
          <input
            type="number"
            min="0"
            step="0.01"
            value={form.precoCusto}
            onChange={(event) =>
              onFormChange((current) => ({ ...current, precoCusto: event.target.value }))
            }
            required
          />
        </label>
        <label>
          Preco venda
          <input
            type="number"
            min="0.01"
            step="0.01"
            value={form.precoVenda}
            onChange={(event) =>
              onFormChange((current) => ({ ...current, precoVenda: event.target.value }))
            }
            required
          />
        </label>
        <button type="submit" className="primary-action" disabled={loading === 'salvar-produto'}>
          <Save aria-hidden="true" size={18} />
          {loading === 'salvar-produto' ? 'Salvando...' : editing ? 'Salvar' : 'Cadastrar'}
        </button>
      </form>

      <section className="admin-products" aria-labelledby="admin-products-title">
        <div className="section-heading">
          <Package aria-hidden="true" size={20} />
          <h2 id="admin-products-title">Cadastro de produtos</h2>
        </div>

        <div className="admin-products-table" role="table" aria-label="Cadastro de produtos">
          <div className="admin-products-row admin-products-head" role="row">
            <span role="columnheader">Produto</span>
            <span role="columnheader">Codigo</span>
            <span role="columnheader">Custo</span>
            <span role="columnheader">Venda</span>
            <span role="columnheader">Status</span>
            <span role="columnheader">Acoes</span>
          </div>
          {products.length ? (
            products.map((produto) => (
              <div className="admin-products-row" role="row" key={produto.id}>
                <span role="cell">
                  <strong>{produto.descricao}</strong>
                  <small>{shortId(produto.id)}</small>
                </span>
                <span role="cell">{produto.codigoBarrasEan ?? '-'}</span>
                <span role="cell">{formatMoney(produto.precoCusto, currency, casasDecimais)}</span>
                <span role="cell">{formatMoney(produto.precoVenda, currency, casasDecimais)}</span>
                <span role="cell">
                  <span className={produto.ativo ? 'status-pill active' : 'status-pill inactive'}>
                    {produto.ativo ? 'Ativo' : 'Inativo'}
                  </span>
                </span>
                <span role="cell" className="row-actions">
                  <button
                    type="button"
                    className="icon-action"
                    onClick={() => onSelectProduct(produto)}
                    title="Editar produto"
                    aria-label={`Editar ${produto.descricao}`}
                  >
                    <Pencil aria-hidden="true" size={17} />
                  </button>
                  <button
                    type="button"
                    className="icon-action danger"
                    onClick={() => onDisableProduct(produto)}
                    title="Inativar produto"
                    aria-label={`Inativar ${produto.descricao}`}
                    disabled={!produto.ativo || loading === `inativar-produto-${produto.id}`}
                  >
                    <Ban aria-hidden="true" size={17} />
                  </button>
                </span>
              </div>
            ))
          ) : (
            <div className="empty-state">Sem produtos para exibir</div>
          )}
        </div>
      </section>
    </div>
  )
}

function AdminReportsView({ reports, loading, currency, casasDecimais, onRefresh }) {
  const summary = reports.summary
  const sales = reports.sales ?? []

  return (
    <div className="admin-pane">
      <div className="admin-header">
        <div className="section-heading">
          <BarChart3 aria-hidden="true" size={20} />
          <h2>Relatorios</h2>
        </div>
        <button
          type="button"
          className="secondary-action"
          onClick={onRefresh}
          disabled={loading === 'relatorios'}
        >
          <RefreshCw aria-hidden="true" size={16} />
          {loading === 'relatorios' ? 'Atualizando...' : 'Atualizar'}
        </button>
      </div>

      <section className="admin-summary" aria-label="Resumo financeiro">
        <Metric label="Vendas" value={String(summary?.quantidadeVendas ?? 0)} />
        <Metric
          label="Valor total"
          value={formatMoney(summary?.valorTotal ?? 0, currency, casasDecimais)}
          strong
        />
        {(summary?.totaisPorFormaPagamento ?? []).map((total) => (
          <Metric
            key={total.formaPagamento}
            label={total.formaPagamento}
            value={formatMoney(total.valorTotal, currency, casasDecimais)}
          />
        ))}
      </section>

      <section className="admin-sales" aria-labelledby="admin-sales-title">
        <div className="section-heading">
          <ReceiptText aria-hidden="true" size={20} />
          <h2 id="admin-sales-title">Vendas concluidas</h2>
        </div>

        <div className="admin-sales-table" role="table" aria-label="Vendas concluidas">
          <div className="admin-sales-row admin-sales-head" role="row">
            <span role="columnheader">Venda</span>
            <span role="columnheader">Pagamento</span>
            <span role="columnheader">Forma</span>
            <span role="columnheader">Itens</span>
            <span role="columnheader">Total</span>
            <span role="columnheader">Concluida</span>
          </div>
          {sales.length ? (
            sales.map((sale) => (
              <div className="admin-sales-row" role="row" key={sale.vendaId}>
                <span role="cell">
                  <strong>{shortId(sale.vendaId)}</strong>
                  <small>Caixa {shortId(sale.caixaId)}</small>
                </span>
                <span role="cell">{shortId(sale.pagamentoId)}</span>
                <span role="cell">{sale.formaPagamento}</span>
                <span role="cell">{formatQuantity(totalSaleItems(sale))}</span>
                <span role="cell">{formatMoney(sale.valorTotal, currency, casasDecimais)}</span>
                <span role="cell">{formatDateTime(sale.concluidaEm)}</span>
              </div>
            ))
          ) : (
            <div className="empty-state">Sem vendas concluidas</div>
          )}
        </div>
      </section>
    </div>
  )
}

function InlineError({ message }) {
  return (
    <div className="inline-error" role="alert">
      <AlertCircle aria-hidden="true" size={18} />
      <span>{message}</span>
    </div>
  )
}

function Metric({ label, value, strong = false }) {
  return (
    <div className={strong ? 'metric metric-strong' : 'metric'}>
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}

async function requestJson(path, { method = 'GET', token, body, notFoundAsNull = false } = {}) {
  const headers = new Headers()
  headers.set('Accept', 'application/json')

  if (body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }

  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(path, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  if (response.status === 404 && notFoundAsNull) {
    return null
  }

  if (!response.ok) {
    throw new Error(await readApiError(response))
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

async function readApiError(response) {
  const fallback = `Erro ${response.status}`
  const contentType = response.headers.get('content-type') ?? ''

  if (!contentType.includes('application/json')) {
    return (await response.text()) || fallback
  }

  const payload = await response.json()
  return payload.mensagem ?? payload.message ?? payload.title ?? payload.detail ?? fallback
}

function readStoredAuth() {
  const stored = sessionStorage.getItem(AUTH_KEY)

  if (!stored) {
    return null
  }

  try {
    return JSON.parse(stored)
  } catch {
    sessionStorage.removeItem(AUTH_KEY)
    return null
  }
}

function setStoredAuth(auth) {
  sessionStorage.setItem(AUTH_KEY, JSON.stringify(auth))
}

function formatMoney(value, currency, casasDecimais) {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency,
    minimumFractionDigits: casasDecimais,
    maximumFractionDigits: casasDecimais,
  }).format(Number(value))
}

function formatNumber(value) {
  return new Intl.NumberFormat('pt-BR', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(Number(value))
}

function shortId(value) {
  return value ? String(value).slice(0, 8) : '-'
}

function totalSaleItems(sale) {
  return sale.itens?.reduce((sum, item) => sum + Number(item.quantidade ?? 0), 0) ?? 0
}

function formatQuantity(value) {
  return new Intl.NumberFormat('pt-BR', {
    maximumFractionDigits: 3,
  }).format(Number(value ?? 0))
}

function formatDateTime(value) {
  if (!value) {
    return '-'
  }

  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value))
}

export default App
