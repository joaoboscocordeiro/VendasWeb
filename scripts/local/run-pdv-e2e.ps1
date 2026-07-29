[CmdletBinding()]
param(
    [switch]$SkipBackend,
    [switch]$SkipFrontend,
    [switch]$SkipBrowserInstall,
    [switch]$Headed,
    [int]$TimeoutSeconds = 90
)

$ErrorActionPreference = 'Stop'

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$RunDir = Join-Path $RepoRoot '.codex-run\frente-caixa-local'
$FrontendDir = Join-Path $RepoRoot 'frontend\pdv'
$FrontendUrl = 'http://127.0.0.1:5173/'

function Write-Step {
    param([string]$Message)

    Write-Host "==> $Message"
}

function Wait-HttpOk {
    param(
        [string]$Url,
        [int]$Timeout = $TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($Timeout)
    $lastError = $null

    do {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5
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

New-Item -ItemType Directory -Force -Path $RunDir | Out-Null
$startedFrontend = $null

try {
    if (!$SkipBackend) {
        Write-Step 'Preparando microsservicos e dados demo'
        & (Join-Path $RepoRoot 'scripts\local\run-microsservicos.ps1') -SkipDemoFlow
        if ($LASTEXITCODE -ne 0) {
            throw 'Falha preparando microsservicos.'
        }
    }

    if (!$SkipFrontend) {
        try {
            Wait-HttpOk -Url $FrontendUrl -Timeout 2 | Out-Null
            Write-Step "Frontend ja esta respondendo em $FrontendUrl"
        } catch {
            $connection = Get-NetTCPConnection -LocalPort 5173 -State Listen -ErrorAction SilentlyContinue
            if ($connection) {
                throw "Porta 5173 esta em uso, mas $FrontendUrl nao respondeu."
            }

            Write-Step "Iniciando frontend PDV em $FrontendUrl"
            $outLog = Join-Path $RunDir 'frontend-pdv-e2e.out.log'
            $errLog = Join-Path $RunDir 'frontend-pdv-e2e.err.log'
            $startedFrontend = Start-Process `
                -FilePath 'npm.cmd' `
                -ArgumentList @('run', 'dev', '--', '--host', '127.0.0.1') `
                -WorkingDirectory $FrontendDir `
                -RedirectStandardOutput $outLog `
                -RedirectStandardError $errLog `
                -WindowStyle Hidden `
                -PassThru

            Wait-HttpOk -Url $FrontendUrl | Out-Null
        }
    }

    if (!$SkipBrowserInstall) {
        Write-Step 'Garantindo browser Chromium do Playwright'
        Push-Location $FrontendDir
        try {
            & npx playwright install chromium
            if ($LASTEXITCODE -ne 0) {
                throw "Playwright install retornou codigo $LASTEXITCODE."
            }
        } finally {
            Pop-Location
        }
    }

    Write-Step 'Executando Playwright E2E do PDV'
    Push-Location $FrontendDir
    try {
        $env:PLAYWRIGHT_BASE_URL = $FrontendUrl
        $arguments = @('playwright', 'test')

        if ($Headed) {
            $arguments += '--headed'
        }

        & npx @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Playwright retornou codigo $LASTEXITCODE."
        }
    } finally {
        Pop-Location
    }
} finally {
    if ($startedFrontend) {
        Write-Step "Parando frontend PDV iniciado para E2E PID $($startedFrontend.Id)"
        Stop-Process -Id $startedFrontend.Id -Force -ErrorAction SilentlyContinue
    }
}
