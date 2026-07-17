import { useEffect, useMemo, useState } from 'react'
import {
  AlertCircle,
  CheckCircle2,
  CircleDollarSign,
  LogIn,
  LogOut,
  Plus,
  RefreshCw,
  ShoppingCart,
  Trash2,
  User,
  Wifi,
  WifiOff,
} from 'lucide-react'
import './App.css'

const AUTH_KEY = 'frente-caixa-pdv-auth'
const emptyGuid = '00000000-0000-0000-0000-000000000000'

const initialLogin = {
  email: 'vendedor@frentecaixa.local',
  senha: 'Senha@123',
}

const initialSaleForm = {
  caixaId: '',
}

const initialItemForm = {
  produtoId: '',
  descricaoProduto: '',
  quantidade: '1',
  precoUnitario: '',
}

function App() {
  const [auth, setAuth] = useState(readStoredAuth)
  const [loginForm, setLoginForm] = useState(initialLogin)
  const [saleForm, setSaleForm] = useState(initialSaleForm)
  const [itemForm, setItemForm] = useState(initialItemForm)
  const [bootstrap, setBootstrap] = useState(null)
  const [venda, setVenda] = useState(null)
  const [loading, setLoading] = useState('')
  const [error, setError] = useState('')

  const isAuthenticated = Boolean(auth?.accessToken)
  const currency = bootstrap?.configuracao?.moeda ?? 'BRL'
  const casasDecimais = bootstrap?.configuracao?.casasDecimais ?? 2

  const totalItens = useMemo(() => {
    return venda?.itens?.reduce((sum, item) => sum + Number(item.quantidade), 0) ?? 0
  }, [venda])

  useEffect(() => {
    if (!isAuthenticated) {
      setBootstrap(null)
      setVenda(null)
      return
    }

    loadBootstrap(auth.accessToken)
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

  async function loadBootstrap(token = auth?.accessToken) {
    if (!token) {
      return
    }

    setError('')
    setLoading('bootstrap')

    try {
      const response = await requestJson('/bff/pdv/bootstrap', { token })
      setBootstrap(response)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function iniciarVenda(event) {
    event.preventDefault()
    setError('')
    setLoading('venda')

    try {
      const response = await requestJson('/vendas/sales', {
        method: 'POST',
        token: auth.accessToken,
        body: {
          caixaId: saleForm.caixaId,
        },
      })
      setVenda(response)
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
      setItemForm({ ...initialItemForm, produtoId: itemForm.produtoId })
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading('')
    }
  }

  async function removerItem(itemId) {
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
    setSaleForm(initialSaleForm)
    setItemForm(initialItemForm)
    setError('')
  }

  function preencherCaixaLocal() {
    setSaleForm((current) => ({ ...current, caixaId: crypto.randomUUID() }))
  }

  function preencherProdutoLocal() {
    setItemForm((current) => ({ ...current, produtoId: crypto.randomUUID() }))
  }

  if (!isAuthenticated) {
    return (
      <main className="login-shell">
        <section className="login-panel" aria-labelledby="login-title">
          <div className="brand-mark">
            <CircleDollarSign aria-hidden="true" size={28} />
          </div>
          <p className="eyebrow">FrenteCaixa</p>
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
          <p className="eyebrow">FrenteCaixa</p>
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
            onClick={() => loadBootstrap()}
            title="Atualizar bootstrap"
            aria-label="Atualizar bootstrap"
            disabled={loading === 'bootstrap'}
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
        <Metric label="Moeda" value={currency} />
        <Metric label="Itens" value={String(totalItens)} />
        <Metric label="Total" value={formatMoney(venda?.total ?? 0, currency, casasDecimais)} strong />
      </section>

      <section className="workspace">
        <div className="sale-pane">
          <div className="section-heading">
            <ShoppingCart aria-hidden="true" size={20} />
            <h2>Venda</h2>
          </div>

          <form className="inline-form" onSubmit={iniciarVenda}>
            <label>
              CaixaId
              <input
                value={saleForm.caixaId}
                onChange={(event) =>
                  setSaleForm((current) => ({ ...current, caixaId: event.target.value }))
                }
                placeholder={emptyGuid}
                required
              />
            </label>
            <button type="button" className="secondary-action" onClick={preencherCaixaLocal}>
              <RefreshCw aria-hidden="true" size={16} />
              Gerar
            </button>
            <button type="submit" className="primary-action" disabled={loading === 'venda'}>
              <Plus aria-hidden="true" size={18} />
              {venda ? 'Nova venda' : 'Iniciar'}
            </button>
          </form>

          <form className="item-form" onSubmit={adicionarItem}>
            <label className="span-2">
              ProdutoId
              <input
                value={itemForm.produtoId}
                onChange={(event) =>
                  setItemForm((current) => ({ ...current, produtoId: event.target.value }))
                }
                placeholder={emptyGuid}
                required
              />
            </label>
            <button type="button" className="secondary-action align-end" onClick={preencherProdutoLocal}>
              <RefreshCw aria-hidden="true" size={16} />
              Gerar
            </button>
            <label className="span-2">
              Descricao
              <input
                value={itemForm.descricaoProduto}
                onChange={(event) =>
                  setItemForm((current) => ({ ...current, descricaoProduto: event.target.value }))
                }
                placeholder="Cafe 500g"
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
              disabled={!venda?.id || loading === 'item'}
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
                      disabled={loading === `remover-${item.id}`}
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

          <div className="checkout-bar">
            <div>
              <span>Total</span>
              <strong>{formatMoney(venda?.total ?? 0, currency, casasDecimais)}</strong>
            </div>
            <button type="button" className="primary-action" disabled>
              <CircleDollarSign aria-hidden="true" size={18} />
              Checkout
            </button>
          </div>
        </div>

        <aside className="context-pane">
          <section aria-labelledby="servicos-title">
            <div className="section-heading">
              <Wifi aria-hidden="true" size={20} />
              <h2 id="servicos-title">Servicos</h2>
            </div>
            <div className="service-list">
              {(bootstrap?.servicos ?? []).map((servico) => (
                <div className="service-card" key={servico.nome}>
                  <div>
                    <strong>{servico.nome}</strong>
                    <span>{servico.statusCode ? `HTTP ${servico.statusCode}` : servico.baseUrl}</span>
                  </div>
                  {servico.status === 'Operacional' ? (
                    <CheckCircle2 className="ok" aria-label="Operacional" size={20} />
                  ) : (
                    <WifiOff className="fail" aria-label="Indisponivel" size={20} />
                  )}
                </div>
              ))}
            </div>
          </section>

          <section aria-labelledby="atalhos-title">
            <div className="section-heading">
              <CheckCircle2 aria-hidden="true" size={20} />
              <h2 id="atalhos-title">Atalhos</h2>
            </div>
            <div className="shortcut-list">
              {(bootstrap?.atalhos ?? []).map((atalho) => (
                <div className="shortcut-row" key={atalho.codigo}>
                  <span>{atalho.rotulo}</span>
                  <code>{atalho.metodo}</code>
                </div>
              ))}
            </div>
          </section>

          <section aria-labelledby="sessao-title">
            <div className="section-heading">
              <User aria-hidden="true" size={20} />
              <h2 id="sessao-title">Sessao</h2>
            </div>
            <dl className="session-details">
              <div>
                <dt>Email</dt>
                <dd>{bootstrap?.usuario?.email ?? auth.usuario?.email}</dd>
              </div>
              <div>
                <dt>Gerado em</dt>
                <dd>{bootstrap?.geradoEm ? formatDate(bootstrap.geradoEm) : '-'}</dd>
              </div>
              <div>
                <dt>Venda</dt>
                <dd>{venda?.status ?? '-'}</dd>
              </div>
            </dl>
          </section>
        </aside>
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

async function requestJson(path, { method = 'GET', token, body } = {}) {
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

function formatDate(value) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value))
}

export default App
