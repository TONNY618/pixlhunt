# =============================================================================
# refresh.ps1 — Шаг 1: обновить карту проекта (dekko + сбор дампов)
# =============================================================================

param(
    [string]$ProjectPath = "D:\git_slavik_rep\pxlhunt\pxlhunt"
)

$ErrorActionPreference = "Stop"

# --- ВАЖНО: force UTF-8 для чтения вывода dekko ---
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  ШАГ 1: Обновление карты проекта" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

Push-Location $ProjectPath

try {
    if (-not (Test-Path "pxlhunt.csproj")) {
        Write-Host "[ОШИБКА] Не найден pxlhunt.csproj в $ProjectPath" -ForegroundColor Red
        exit 1
    }

    $dekko = Get-Command dekko -ErrorAction SilentlyContinue
    if (-not $dekko) {
        Write-Host "[ОШИБКА] dekko не найден в PATH." -ForegroundColor Red
        exit 1
    }

    Write-Host "[1/2] Запускаю dekko map..." -ForegroundColor Yellow
    Write-Host ""
    dekko map
    Write-Host ""

    Write-Host "[2/2] Собираю дампы в docs\code-map\..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path "docs\code-map"       | Out-Null
    New-Item -ItemType Directory -Force -Path "docs\code-map\pages" | Out-Null

    dekko summary                 | Out-File -Encoding utf8 "docs\code-map\_summary.txt"
    dekko deps                    | Out-File -Encoding utf8 "docs\code-map\_deps.txt"
    dekko stats                   | Out-File -Encoding utf8 "docs\code-map\_stats.txt"
    dekko export --format mermaid | Out-File -Encoding utf8 "docs\code-map\callgraph.mmd"

    Copy-Item ".dekko\MAP.md"    "docs\code-map\MAP.md" -Force
    Copy-Item ".dekko\map\*.md"  "docs\code-map\pages\" -Force

    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "  ГОТОВО. Артефакты в docs\code-map\" -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host ""

    Get-ChildItem "docs\code-map"       | Select-Object Name, Length | Format-Table -AutoSize
    Write-Host "Страниц в pages\: $((Get-ChildItem 'docs\code-map\pages').Count)"
    Write-Host ""
    Write-Host "Время: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
}
finally {
    Pop-Location
}
