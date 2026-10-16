# =============================================================================
# build-overview.ps1 — Шаг 3: генерация PROJECT_OVERVIEW.md через DeepSeek
# =============================================================================
# Берёт AGENT_PACK.md, отправляет вместе с промптом в DeepSeek через Aider,
# сохраняет ТОЛЬКО чистый ответ модели в docs/code-map/PROJECT_OVERVIEW.md.
# Обрезает служебный вывод Aider (Can't initialize..., Repo-map..., и т.д.).
# =============================================================================

param(
    [string]$ProjectPath = "D:\git_slavik_rep\pxlhunt\pxlhunt",
    [string]$Model       = "deepseek/deepseek-chat"
)

$ErrorActionPreference = "Stop"

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  ШАГ 3: Генерация PROJECT_OVERVIEW.md" -ForegroundColor Cyan
Write-Host "  Модель: $Model" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

Push-Location $ProjectPath
try {
    $packFile   = "docs\code-map\AGENT_PACK.md"
    $promptFile = "map\overview_prompt.txt"
    $outputFile = "docs\code-map\PROJECT_OVERVIEW.md"

    if (-not (Test-Path $packFile)) {
        Write-Host "[ОШИБКА] Нет $packFile. Сначала запустите build-agent-pack.ps1" -ForegroundColor Red
        exit 1
    }
    if (-not (Test-Path $promptFile)) {
        Write-Host "[ОШИБКА] Нет $promptFile." -ForegroundColor Red
        exit 1
    }

    $packSize = (Get-Item $packFile).Length
    Write-Host "Вход: $packFile ($packSize байт)" -ForegroundColor Yellow
    Write-Host "Промпт: $promptFile" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Отправляю в DeepSeek... (может занять 30-90 секунд)" -ForegroundColor Yellow
    Write-Host ""

    $prompt = Get-Content $promptFile -Raw -Encoding UTF8

    $cmd = @(
        "aider",
        "--model", $Model,
        "--chat-mode", "ask",
        "--no-pretty",
        "--no-auto-commits",
        "--no-dirty-commits",
        "--no-show-model-warnings",
        "--no-check-update",
        "--no-show-release-notes",
        "--yes",
        "--read", $packFile,
        "--message", $prompt
    )

    $rawOutput = & aider @($cmd[1..($cmd.Length-1)]) 2>&1 | Out-String

    # --- Очистка вывода Aider: оставляем только чистый ответ модели ---
    $cleaned = $rawOutput

    # 1. Отрезаем всё до первого "► **ANSWER**" или "**ANSWER**", если они есть
    foreach ($marker in @("► **ANSWER**", "**ANSWER**")) {
        if ($cleaned -match [regex]::Escape($marker)) {
            $cleaned = $cleaned.Substring($cleaned.IndexOf($marker) + $marker.Length)
            break
        }
    }

    # 2. Отрезаем "Tokens:" и всё после него
    if ($cleaned -match "Tokens:") {
        $cleaned = $cleaned.Substring(0, $cleaned.IndexOf("Tokens:"))
    }

    # 3. ГЛАВНОЕ: находим первую Markdown-строку "# " и берём всё от неё
    $lines = $cleaned -split "`r?`n"
    $startIdx = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^#\s') {
            $startIdx = $i
            break
        }
    }
    if ($startIdx -ge 0) {
        $cleaned = ($lines[$startIdx..($lines.Count-1)] -join "`n")
    }

    # 4. Обрезаем хвостовые пустые строки и лишние пробелы
    $cleaned = $cleaned.TrimEnd()

    Set-Content -Path $outputFile -Value $cleaned -Encoding UTF8

    $outSize = (Get-Item $outputFile).Length
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "  ГОТОВО: $outputFile" -ForegroundColor Green
    Write-Host "  Размер: $outSize байт" -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Green
}
finally {
    Pop-Location
}
