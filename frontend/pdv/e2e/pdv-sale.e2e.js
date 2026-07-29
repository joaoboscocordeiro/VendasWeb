import { expect, test } from '@playwright/test'

const demoBarcode = process.env.PDV_DEMO_BARCODE ?? '7891000000015'
const sellerEmail = process.env.PDV_SELLER_EMAIL ?? 'vendedor@frentecaixa.local'
const sellerPassword = process.env.PDV_SELLER_PASSWORD ?? 'Senha@123'

test.describe('PDV venda completa', () => {
  test('operador conclui uma venda demo e ve o resumo do caixa atualizar', async ({ page }) => {
    await page.goto('/')

    await page.getByLabel('Email').fill(sellerEmail)
    await page.getByLabel('Senha').fill(sellerPassword)
    await page.getByRole('button', { name: 'Entrar' }).click()

    await expect(page.getByRole('heading', { name: 'Venda' })).toBeVisible()
    await ensureCashRegisterOpen(page)

    const summary = page.getByLabel('Resumo do turno')
    const totalSold = summary.locator('.metric', { hasText: 'Total vendido' }).locator('strong')
    const totalBefore = parseBrl(await totalSold.textContent())

    await page.getByRole('button', { name: /^(Iniciar|Nova venda)$/ }).click()
    await expect(page.getByText('Venda EmAndamento')).toBeVisible()

    await page.getByLabel('Codigo de barras').fill(demoBarcode)
    await page.getByRole('button', { name: 'Localizar' }).click()

    await expect(page.getByRole('button', { name: /Cafe Torrado Demo 500g/ })).toBeVisible()
    await expect(page.getByRole('button', { name: 'Adicionar item' })).toBeEnabled()
    await page.getByRole('button', { name: 'Adicionar item' }).click()

    const itemsTable = page.getByRole('table', { name: 'Itens da venda' })
    await expect(itemsTable).toContainText('Cafe Torrado Demo 500g')
    await expect(itemsTable).toContainText('R$ 14,90')

    await page.getByLabel('Valor recebido').fill('14.90')
    await page.getByRole('button', { name: 'Checkout' }).click()

    const checkoutReceipt = page.getByRole('status').filter({ hasText: 'Venda concluida' })
    await expect(checkoutReceipt).toBeVisible()
    await expect(checkoutReceipt.getByText(/^Pagamento /)).toBeVisible()
    await expect
      .poll(async () => {
        await page.getByRole('button', { name: 'Atualizar contexto do PDV' }).click()
        await page.waitForTimeout(500)

        return parseBrl(await totalSold.textContent())
      }, {
        message: 'total vendido do resumo do caixa deve aumentar apos checkout',
        timeout: 30_000,
      })
      .toBeGreaterThan(totalBefore)
  })
})

async function ensureCashRegisterOpen(page) {
  const summary = page.getByLabel('Resumo do turno')
  const openCashButton = page.getByRole('button', { name: 'Abrir caixa' })

  if (await summary.isVisible({ timeout: 5_000 }).catch(() => false)) {
    return
  }

  if (await openCashButton.isVisible({ timeout: 5_000 }).catch(() => false)) {
    await expect(openCashButton).toBeEnabled()
    await page.getByLabel('Valor inicial').fill('50')
    await openCashButton.click()
  }

  await expect(summary).toBeVisible()
}

function parseBrl(value) {
  return Number(
    String(value ?? '0')
      .replace(/[^\d,.-]/g, '')
      .replace(/\./g, '')
      .replace(',', '.'),
  )
}
