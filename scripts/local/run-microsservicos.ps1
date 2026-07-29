[CmdletBinding()]
param(
    [switch]$SkipInfra,
    [switch]$SkipBuild,
    [switch]$SkipMigrations,
    [switch]$SkipSeed,
    [switch]$SkipDemoFlow,
    [switch]$HealthOnly,
    [switch]$Stop,
    [int]$TimeoutSeconds = 90
)

$ErrorActionPreference = 'Stop'

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$RunDir = Join-Path $RepoRoot '.codex-run\frente-caixa-local'
$ProcessFile = Join-Path $RunDir 'processes.json'
$JwtKey = 'frente-caixa-dev-local-chave-compartilhada-para-todos-os-servicos'
$DemoProducts = @(
    @{
        Id = '33333333-3333-3333-3333-333333333333'
        Descricao = 'Cafe Torrado Demo 500g'
        CodigoBarrasEan = '7891000000015'
        PrecoCusto = '9.50'
        PrecoVenda = '14.90'
        Estoque = '200.000'
        MovimentoSeedId = '66666666-6666-6666-6666-666666666661'
    },
    @{
        Id = '44444444-4444-4444-4444-444444444444'
        Descricao = 'Leite Integral Demo 1L'
        CodigoBarrasEan = '7891000000022'
        PrecoCusto = '3.40'
        PrecoVenda = '5.99'
        Estoque = '200.000'
        MovimentoSeedId = '66666666-6666-6666-6666-666666666662'
    },
    @{
        Id = '55555555-5555-5555-5555-555555555555'
        Descricao = 'Pao Frances Demo Kg'
        CodigoBarrasEan = '7891000000039'
        PrecoCusto = '8.80'
        PrecoVenda = '15.50'
        Estoque = '200.000'
        MovimentoSeedId = '66666666-6666-6666-6666-666666666663'
    }
)

$Services = @(
    @{
        Name = 'Identidade'
        Port = 5227
        Project = 'services\Identidade\src\FrenteCaixa.Identidade.Api\FrenteCaixa.Identidade.Api.csproj'
        MigrationProject = 'services\Identidade\src\FrenteCaixa.Identidade.Infrastructure\FrenteCaixa.Identidade.Infrastructure.csproj'
        DbContext = 'IdentidadeDbContext'
    },
    @{
        Name = 'CatalogoProdutos'
        Port = 5089
        Project = 'services\CatalogoProdutos\src\FrenteCaixa.CatalogoProdutos.Api\FrenteCaixa.CatalogoProdutos.Api.csproj'
        MigrationProject = 'services\CatalogoProdutos\src\FrenteCaixa.CatalogoProdutos.Infrastructure\FrenteCaixa.CatalogoProdutos.Infrastructure.csproj'
        DbContext = 'CatalogoProdutosDbContext'
    },
    @{
        Name = 'Estoque'
        Port = 5252
        Project = 'services\Estoque\src\FrenteCaixa.Estoque.Api\FrenteCaixa.Estoque.Api.csproj'
        MigrationProject = 'services\Estoque\src\FrenteCaixa.Estoque.Infrastructure\FrenteCaixa.Estoque.Infrastructure.csproj'
        DbContext = 'EstoqueDbContext'
    },
    @{
        Name = 'Caixa'
        Port = 5062
        Project = 'services\Caixa\src\FrenteCaixa.Caixa.Api\FrenteCaixa.Caixa.Api.csproj'
        MigrationProject = 'services\Caixa\src\FrenteCaixa.Caixa.Infrastructure\FrenteCaixa.Caixa.Infrastructure.csproj'
        DbContext = 'CaixaDbContext'
    },
    @{
        Name = 'Vendas'
        Port = 5165
        Project = 'services\Vendas\src\FrenteCaixa.Vendas.Api\FrenteCaixa.Vendas.Api.csproj'
        MigrationProject = 'services\Vendas\src\FrenteCaixa.Vendas.Infrastructure\FrenteCaixa.Vendas.Infrastructure.csproj'
        DbContext = 'VendasDbContext'
    },
    @{
        Name = 'Pagamentos'
        Port = 5216
        Project = 'services\Pagamentos\src\FrenteCaixa.Pagamentos.Api\FrenteCaixa.Pagamentos.Api.csproj'
        MigrationProject = 'services\Pagamentos\src\FrenteCaixa.Pagamentos.Infrastructure\FrenteCaixa.Pagamentos.Infrastructure.csproj'
        DbContext = 'PagamentosDbContext'
    },
    @{
        Name = 'Relatorios'
        Port = 5002
        Project = 'services\Relatorios\src\FrenteCaixa.Relatorios.Api\FrenteCaixa.Relatorios.Api.csproj'
        MigrationProject = 'services\Relatorios\src\FrenteCaixa.Relatorios.Infrastructure\FrenteCaixa.Relatorios.Infrastructure.csproj'
        DbContext = 'RelatoriosDbContext'
    },
    @{
        Name = 'BFF'
        Port = 5265
        Project = 'services\Bff\src\FrenteCaixa.Bff.Api\FrenteCaixa.Bff.Api.csproj'
    }
)

function Write-Step {
    param([string]$Message)

    Write-Host "==> $Message"
}

function Get-ServiceUrl {
    param([hashtable]$Service)

    "http://127.0.0.1:$($Service.Port)"
}

function Wait-HttpOk {
    param(
        [string]$Url,
        [int]$Timeout = $TimeoutSeconds,
        [hashtable]$Headers = @{}
    )

    $deadline = (Get-Date).AddSeconds($Timeout)
    $lastError = $null

    do {
        try {
            $response = Invoke-WebRequest -Uri $Url -Headers $Headers -UseBasicParsing -TimeoutSec 5
            if ([int]$response.StatusCode -ge 200 -and [int]$response.StatusCode -lt 300) {
                return $response
            }
        } catch {
            $lastError = $_
            Start-Sleep -Milliseconds 500
        }
    } while ((Get-Date) -lt $deadline)

    if ($lastError) {
        throw "Timeout aguardando $Url. Ultimo erro: $($lastError.Exception.Message)"
    }

    throw "Timeout aguardando $Url."
}

function Wait-ContainerHealthy {
    param(
        [string]$Name,
        [int]$Timeout = $TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($Timeout)

    do {
        $status = docker inspect --format '{{.State.Health.Status}}' $Name 2>$null
        if ($LASTEXITCODE -eq 0 -and $status -eq 'healthy') {
            return
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    throw "Container $Name nao ficou healthy dentro de $Timeout segundos."
}

function Stop-RecordedProcesses {
    if (!(Test-Path $ProcessFile)) {
        Write-Step "Nenhum arquivo de processos encontrado em $ProcessFile"
        return
    }

    $records = Get-Content $ProcessFile -Raw | ConvertFrom-Json

    foreach ($record in $records) {
        $process = Get-Process -Id $record.ProcessId -ErrorAction SilentlyContinue
        if ($process) {
            Write-Step "Parando $($record.Name) PID $($record.ProcessId)"
            Stop-Process -Id $record.ProcessId -Force
        }
    }

    Remove-Item -LiteralPath $ProcessFile -Force
}

function Invoke-CommandChecked {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$WorkingDirectory = $RepoRoot
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Comando falhou: $FilePath $($Arguments -join ' ')"
    }
}

function Start-ServiceProcess {
    param([hashtable]$Service)

    $url = Get-ServiceUrl $Service
    $healthUrl = "$url/api/saude"

    try {
        Wait-HttpOk -Url $healthUrl -Timeout 2 | Out-Null
        Write-Step "$($Service.Name) ja esta respondendo em $healthUrl"
        return $null
    } catch {
        $connection = Get-NetTCPConnection -LocalPort $Service.Port -State Listen -ErrorAction SilentlyContinue
        if ($connection) {
            throw "Porta $($Service.Port) esta em uso, mas $healthUrl nao respondeu."
        }
    }

    $outLog = Join-Path $RunDir "$($Service.Name).out.log"
    $errLog = Join-Path $RunDir "$($Service.Name).err.log"
    $projectPath = Join-Path $RepoRoot $Service.Project
    $projectDirectory = Split-Path $projectPath -Parent
    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
    $assemblyPath = Join-Path $projectDirectory "bin\Debug\net10.0\$assemblyName.dll"

    if (!(Test-Path $assemblyPath)) {
        throw "Assembly nao encontrado para $($Service.Name): $assemblyPath. Execute o script sem -SkipBuild."
    }

    $arguments = @(
        $assemblyPath,
        '--urls',
        $url
    )

    Write-Step "Iniciando $($Service.Name) em $url"
    $process = Start-Process `
        -FilePath 'dotnet' `
        -ArgumentList $arguments `
        -WorkingDirectory $projectDirectory `
        -RedirectStandardOutput $outLog `
        -RedirectStandardError $errLog `
        -WindowStyle Hidden `
        -PassThru

    [PSCustomObject]@{
        Name = $Service.Name
        ProcessId = $process.Id
        Url = $url
        StartedAt = (Get-Date).ToString('O')
    }
}

function Update-Database {
    param([hashtable]$Service)

    if (!$Service.ContainsKey('MigrationProject')) {
        return
    }

    Write-Step "Aplicando migrations de $($Service.Name)"
    Invoke-CommandChecked -FilePath 'dotnet' -Arguments @(
        'ef',
        'database',
        'update',
        '--project',
        (Join-Path $RepoRoot $Service.MigrationProject),
        '--startup-project',
        (Join-Path $RepoRoot $Service.Project),
        '--context',
        $Service.DbContext,
        '--no-build'
    )
}

function New-PasswordHash {
    param([string]$Password)

    $iterations = 210000
    $salt = New-Object byte[] 16
    $random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $random.GetBytes($salt)
    $random.Dispose()
    $pbkdf2 = [System.Security.Cryptography.Rfc2898DeriveBytes]::new(
        $Password,
        $salt,
        $iterations,
        [System.Security.Cryptography.HashAlgorithmName]::SHA256)
    $hash = $pbkdf2.GetBytes(32)
    $pbkdf2.Dispose()

    $saltBase64 = [Convert]::ToBase64String($salt)
    $hashBase64 = [Convert]::ToBase64String($hash)

    "pbkdf2-sha256`$$iterations`$$saltBase64`$$hashBase64"
}

function Invoke-PostgresSql {
    param(
        [string]$Sql,
        [string]$Database = 'frente_caixa_identidade'
    )

    $tempFile = Join-Path $RunDir "seed-$([Guid]::NewGuid()).sql"
    Set-Content -LiteralPath $tempFile -Value $Sql -Encoding UTF8

    try {
        Get-Content -LiteralPath $tempFile | docker exec -i frente-caixa-postgres psql -U frente_caixa -d $Database -v ON_ERROR_STOP=1
        if ($LASTEXITCODE -ne 0) {
            throw "psql retornou codigo $LASTEXITCODE"
        }
    } finally {
        Remove-Item -LiteralPath $tempFile -Force -ErrorAction SilentlyContinue
    }
}

function Seed-DevUsers {
    Write-Step "Semeando usuarios dev em Identidade"

    $now = (Get-Date).ToUniversalTime().ToString('O')
    $adminHash = New-PasswordHash 'Senha@123'
    $sellerHash = New-PasswordHash 'Senha@123'
    $sql = @"
INSERT INTO usuarios ("Id", "Nome", "Email", "SenhaHash", "Perfil", "Ativo", "CriadoEm", "AtualizadoEm")
VALUES
  ('11111111-1111-1111-1111-111111111111', 'Administrador Local', 'admin@frentecaixa.local', '$adminHash', 'ADM', TRUE, '$now', '$now'),
  ('22222222-2222-2222-2222-222222222222', 'Vendedor Local', 'vendedor@frentecaixa.local', '$sellerHash', 'VENDEDOR', TRUE, '$now', '$now')
ON CONFLICT ("Email") DO UPDATE
SET "SenhaHash" = EXCLUDED."SenhaHash",
    "Ativo" = TRUE,
    "AtualizadoEm" = EXCLUDED."AtualizadoEm";
"@

    Invoke-PostgresSql $sql | Out-Null
}

function Seed-DemoProducts {
    Write-Step "Semeando produtos demo em CatalogoProdutos e Estoque"

    $now = (Get-Date).ToUniversalTime().ToString('O')
    $catalogRows = $DemoProducts | ForEach-Object {
        "  ('$($_.Id)', '$($_.Descricao)', '$($_.CodigoBarrasEan)', $($_.PrecoCusto), $($_.PrecoVenda), TRUE, '$now', '$now')"
    }
    $stockRows = $DemoProducts | ForEach-Object {
        "  ('$($_.Id)', $($_.Estoque), '$now')"
    }
    $movementRows = $DemoProducts | ForEach-Object {
        "  ('$($_.MovimentoSeedId)', '$($_.Id)', 'Entrada', $($_.Estoque), 0, $($_.Estoque), 'Seed demo local', '$now')"
    }

    $catalogSql = @"
INSERT INTO produtos ("Id", "Descricao", "CodigoBarrasEan", "PrecoCusto", "PrecoVenda", "Ativo", "CriadoEm", "AtualizadoEm")
VALUES
$($catalogRows -join ",`n")
ON CONFLICT ("Id") DO UPDATE
SET "Descricao" = EXCLUDED."Descricao",
    "CodigoBarrasEan" = EXCLUDED."CodigoBarrasEan",
    "PrecoCusto" = EXCLUDED."PrecoCusto",
    "PrecoVenda" = EXCLUDED."PrecoVenda",
    "Ativo" = TRUE,
    "AtualizadoEm" = EXCLUDED."AtualizadoEm";
"@

    $stockSql = @"
INSERT INTO saldos_produtos ("ProdutoId", "QuantidadeDisponivel", "AtualizadoEm")
VALUES
$($stockRows -join ",`n")
ON CONFLICT ("ProdutoId") DO UPDATE
SET "QuantidadeDisponivel" = GREATEST(saldos_produtos."QuantidadeDisponivel", EXCLUDED."QuantidadeDisponivel"),
    "AtualizadoEm" = EXCLUDED."AtualizadoEm";

INSERT INTO movimentacoes_estoque ("Id", "ProdutoId", "Tipo", "Quantidade", "QuantidadeAnterior", "QuantidadeAtual", "Motivo", "CriadaEm")
VALUES
$($movementRows -join ",`n")
ON CONFLICT ("Id") DO NOTHING;
"@

    Invoke-PostgresSql -Database 'frente_caixa_catalogo' -Sql $catalogSql | Out-Null
    Invoke-PostgresSql -Database 'frente_caixa_estoque' -Sql $stockSql | Out-Null
}

function Test-HealthChecks {
    foreach ($service in $Services) {
        $healthUrl = "$(Get-ServiceUrl $service)/api/saude"
        Wait-HttpOk -Url $healthUrl | Out-Null
        Write-Step "$($service.Name) OK em $healthUrl"
    }
}

function Invoke-AuthenticatedSmoke {
    $loginBody = @{
        email = 'vendedor@frentecaixa.local'
        senha = 'Senha@123'
    } | ConvertTo-Json

    Write-Step "Validando login local"
    $login = Invoke-RestMethod `
        -Uri 'http://127.0.0.1:5227/auth/login' `
        -Method Post `
        -Body $loginBody `
        -ContentType 'application/json' `
        -TimeoutSec 10

    if ([string]::IsNullOrWhiteSpace($login.accessToken)) {
        throw 'Login nao retornou accessToken.'
    }

    Write-Step "Validando bootstrap do PDV via BFF"
    $headers = @{ Authorization = "Bearer $($login.accessToken)" }
    $bootstrap = Invoke-RestMethod `
        -Uri 'http://127.0.0.1:5265/pdv/bootstrap' `
        -Headers $headers `
        -TimeoutSec 10

    if (!$bootstrap.servicos -or $bootstrap.servicos.Count -lt 7) {
        throw 'Bootstrap do PDV nao retornou a lista esperada de servicos.'
    }

    Write-Step "Smoke autenticado OK para $($bootstrap.usuario.email)"
}

function Wait-Until {
    param(
        [scriptblock]$Condition,
        [string]$TimeoutMessage,
        [int]$Timeout = $TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($Timeout)
    $lastError = $null

    do {
        try {
            $result = & $Condition
            if ($result) {
                return $result
            }
        } catch {
            $lastError = $_
        }

        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    if ($lastError) {
        throw "$TimeoutMessage Ultimo erro: $($lastError.Exception.Message)"
    }

    throw $TimeoutMessage
}

function Invoke-DemoSaleSmoke {
    $demoProduct = $DemoProducts[0]
    $loginBody = @{
        email = 'vendedor@frentecaixa.local'
        senha = 'Senha@123'
    } | ConvertTo-Json

    Write-Step "Validando venda completa demo"
    $login = Invoke-RestMethod `
        -Uri 'http://127.0.0.1:5227/auth/login' `
        -Method Post `
        -Body $loginBody `
        -ContentType 'application/json' `
        -TimeoutSec 10

    $headers = @{ Authorization = "Bearer $($login.accessToken)" }
    $product = Invoke-RestMethod `
        -Uri "http://127.0.0.1:5265/pdv/products/by-barcode/$($demoProduct.CodigoBarrasEan)" `
        -Headers $headers `
        -TimeoutSec 10

    if ($product.id -ne $demoProduct.Id) {
        throw "Produto demo retornado difere do esperado: $($product.id)"
    }

    $stockBefore = Invoke-RestMethod `
        -Uri "http://127.0.0.1:5252/stock/products/$($product.id)" `
        -Headers $headers `
        -TimeoutSec 10

    try {
        $cash = Invoke-RestMethod `
            -Uri 'http://127.0.0.1:5265/pdv/cash-register/current' `
            -Headers $headers `
            -TimeoutSec 10
    } catch {
        $cash = Invoke-RestMethod `
            -Uri 'http://127.0.0.1:5265/pdv/cash-register/open' `
            -Method Post `
            -Headers $headers `
            -Body (@{ valorInicial = 50 } | ConvertTo-Json) `
            -ContentType 'application/json' `
            -TimeoutSec 10
    }

    $summaryBefore = Invoke-RestMethod `
        -Uri 'http://127.0.0.1:5265/pdv/cash-register/current/summary' `
        -Headers $headers `
        -TimeoutSec 10

    $sale = Invoke-RestMethod `
        -Uri 'http://127.0.0.1:5165/sales' `
        -Method Post `
        -Headers $headers `
        -Body (@{ caixaId = $cash.id } | ConvertTo-Json) `
        -ContentType 'application/json' `
        -TimeoutSec 10

    $sale = Invoke-RestMethod `
        -Uri "http://127.0.0.1:5165/sales/$($sale.id)/items" `
        -Method Post `
        -Headers $headers `
        -Body (@{
            produtoId = $product.id
            descricaoProduto = $product.descricao
            quantidade = 1
            precoUnitario = $product.precoVenda
        } | ConvertTo-Json) `
        -ContentType 'application/json' `
        -TimeoutSec 10

    $checkout = Invoke-RestMethod `
        -Uri "http://127.0.0.1:5165/sales/$($sale.id)/checkout" `
        -Method Post `
        -Headers $headers `
        -Body (@{
            formaPagamento = 'Dinheiro'
            valorPago = $sale.total
        } | ConvertTo-Json) `
        -ContentType 'application/json' `
        -TimeoutSec 15

    if ($checkout.venda.status -ne 'Concluida') {
        throw "Checkout demo nao concluiu a venda $($sale.id)."
    }

    $stockAfter = Invoke-RestMethod `
        -Uri "http://127.0.0.1:5252/stock/products/$($product.id)" `
        -Headers $headers `
        -TimeoutSec 10

    $expectedStock = [decimal]$stockBefore.quantidadeDisponivel - 1
    if ([decimal]$stockAfter.quantidadeDisponivel -ne $expectedStock) {
        throw "Estoque demo nao baixou como esperado. Antes=$($stockBefore.quantidadeDisponivel), depois=$($stockAfter.quantidadeDisponivel)."
    }

    $summaryAfter = Wait-Until `
        -TimeoutMessage "Resumo do caixa nao refletiu a venda demo $($sale.id)." `
        -Condition {
            $summary = Invoke-RestMethod `
                -Uri 'http://127.0.0.1:5265/pdv/cash-register/current/summary' `
                -Headers $headers `
                -TimeoutSec 10

            $quantityIncreased = [int]$summary.quantidadeVendas -gt [int]$summaryBefore.quantidadeVendas
            $totalIncreased = [decimal]$summary.totalVendido -ge ([decimal]$summaryBefore.totalVendido + [decimal]$sale.total)

            if ($quantityIncreased -and $totalIncreased) {
                return $summary
            }

            return $null
        }

    $adminLogin = Invoke-RestMethod `
        -Uri 'http://127.0.0.1:5227/auth/login' `
        -Method Post `
        -Body (@{
            email = 'admin@frentecaixa.local'
            senha = 'Senha@123'
        } | ConvertTo-Json) `
        -ContentType 'application/json' `
        -TimeoutSec 10

    $adminHeaders = @{ Authorization = "Bearer $($adminLogin.accessToken)" }
    $reportedSale = Wait-Until `
        -TimeoutMessage "Relatorios nao refletiram a venda demo $($sale.id)." `
        -Condition {
            $sales = Invoke-RestMethod `
                -Uri 'http://127.0.0.1:5265/admin/reports/sales' `
                -Headers $adminHeaders `
                -TimeoutSec 10

            $sales | Where-Object { $_.vendaId -eq $sale.id } | Select-Object -First 1
        }

    Write-Step "Smoke venda demo OK para $($product.descricao) na venda $($sale.id)"
}

Push-Location $RepoRoot
try {
    New-Item -ItemType Directory -Force -Path $RunDir | Out-Null

    if ($Stop) {
        Stop-RecordedProcesses
        return
    }

    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:Jwt__Chave = $JwtKey

    if (!$SkipInfra -and !$HealthOnly) {
        Write-Step 'Subindo infraestrutura Docker'
        Invoke-CommandChecked -FilePath 'docker' -Arguments @('compose', 'up', '-d', 'postgres', 'rabbitmq')
        Wait-ContainerHealthy 'frente-caixa-postgres'
        Wait-ContainerHealthy 'frente-caixa-rabbitmq'
    }

    if (!$SkipBuild -and !$HealthOnly) {
        Write-Step 'Compilando solucao uma vez'
        Invoke-CommandChecked -FilePath 'dotnet' -Arguments @('build', 'FrenteCaixa.sln', '--no-restore')
    }

    if (!$SkipMigrations -and !$HealthOnly) {
        foreach ($service in $Services) {
            Update-Database $service
        }
    }

    if (!$SkipSeed -and !$HealthOnly) {
        Seed-DevUsers
        Seed-DemoProducts
    }

    $started = @()

    if (!$HealthOnly) {
        foreach ($service in $Services) {
            $record = Start-ServiceProcess $service
            if ($record) {
                $started += $record
            }
        }

        if ($started.Count -gt 0) {
            $started | ConvertTo-Json | Set-Content -LiteralPath $ProcessFile -Encoding UTF8
        }
    }

    Test-HealthChecks
    Invoke-AuthenticatedSmoke
    if (!$HealthOnly -and !$SkipDemoFlow) {
        Invoke-DemoSaleSmoke
    }

    Write-Host ''
    Write-Host 'status = ok'
    Write-Host "logs = $RunDir"
    Write-Host 'frontend = http://127.0.0.1:5173/ (inicie com npm run dev em frontend/pdv quando necessario)'
} finally {
    Pop-Location
}
