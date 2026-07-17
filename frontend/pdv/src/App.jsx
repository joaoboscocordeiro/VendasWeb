import { useEffect, useMemo, useState } from 'react'
import {
  AlertCircle,
  CircleDollarSign,
  LogIn,
  LogOut,
  Plus,
  RefreshCw,
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
  const [venda, setVenda] = useState(null)
  const [loading, setLoading] = useState('')
  const [error, setError] = useState('')

  const isAuthenticated = Boolean(auth?.accessToken)
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

  const totalItens = useMemo(() => {
    return venda?.itens?.reduce((sum, item) => sum + Number(item.quantidade), 0) ?? 0
  }, [venda])

  useEffect(() => {
    if (!isAuthenticated) {
      setBootstrap(null)
      setCaixaAtual(null)
      setMovimentacoesCaixa([])
      setVenda(null)
      setCheckoutResult(null)
      setProductResults([])
      setSelectedProduct(null)
      return
    }

    loadPdvContext(auth.accessToken)
  }, [auth?.accessToken, isAuthenticated])

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
        const movimentacoes = await requestJson('/bff/pdv/cash-register/current/movements', { token })
        setMovimentacoesCaixa(movimentacoes)
      } else {
        setMovimentacoesCaixa([])
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
      setMovimentacoesCaixa(movimentacoes)
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
    setError('')
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

      {error && <InlineError message={error} />}

      <section className="status-band" aria-label="Status do PDV">
        <Metric label="Status" value={loading ? 'Sincronizando' : 'Operacional'} />
        <Metric label="Perfil" value={bootstrap?.usuario?.perfil ?? auth.usuario?.perfil ?? '-'} />
        <Metric label="Caixa" value={caixaAtual?.status ?? 'Fechado'} />
        <Metric label="Itens" value={String(totalItens)} />
        <Metric label="Total" value={formatMoney(venda?.total ?? 0, currency, casasDecimais)} strong />
      </section>

      <section className="workspace">
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

      </section>
    </main>
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

export default App
