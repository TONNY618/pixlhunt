# =============================================================================
# build-agent-pack.ps1 — Шаг 2: собрать AGENT_PACK.md
# =============================================================================
# Собирает единый файл-контекст для AI-агента из артефактов Шага 1.
# При первом запуске создаёт шаблон _agent_pack_header.md — отредактируйте
# его под себя, потом просто запускайте скрипт снова.
# =============================================================================

param(
    [string]$ProjectPath = "D:\git_slavik_rep\pxlhunt\pxlhunt"
)

$ErrorActionPreference = "Stop"
Push-Location $ProjectPath

try {
    $headerFile = "docs\code-map\_agent_pack_header.md"
    $outputFile = "docs\code-map\AGENT_PACK.md"

    if (-not (Test-Path "docs\code-map\_summary.txt")) {
        Write-Host "[ОШИБКА] Нет docs\code-map\_summary.txt. Сначала запустите refresh.ps1" -ForegroundColor Red
        exit 1
    }

    # Создаём шаблон header при первом запуске
    if (-not (Test-Path $headerFile)) {
        Write-Host "[INFO] Создаю шаблон $headerFile" -ForegroundColor Yellow

        $template = @"
# pxlhunt - контекст для AI-агента

## О проекте
Windows-приложение на C# (.NET WinForms) для автоматизации действий:
захват экрана, распознавание триггеров (цвет, клавиши, мышь, таймер),
выполнение сценариев через Arduino HID-устройство.

## Архитектура (слои)
- Core/     - ядро: сервисы, модели, абстракции, захват, ввод
- Engine/   - оркестрация: конфиги, комбинации, запуск триггеров
- Triggers/ - триггеры и шаги выполнения
- UI/       - WinForms-интерфейс

## Ключевые абстракции
- IComboStep              - интерфейс одного шага комбинации
- ITriggerEvaluator       - интерфейс оценки триггера
- Orchestrator            - главный координатор
- TriggerExecutionContext - контекст выполнения

## Как читать эту карту
- Сводка, зависимости, статистика - в конце файла.
- Детализация по папкам - в docs/code-map/pages/*.md
"@
        Set-Content -Path $headerFile -Value $template -Encoding UTF8
        Write-Host "[INFO] Шаблон создан. Отредактируйте его под себя при желании и запустите скрипт снова." -ForegroundColor Green
    }

    Write-Host "Собираю AGENT_PACK.md..." -ForegroundColor Yellow

    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine((Get-Content $headerFile -Raw -Encoding UTF8))
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("---")
    [void]$sb.AppendLine()

    [void]$sb.AppendLine("## Сводка (dekko summary)")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine((Get-Content "docs\code-map\_summary.txt" -Raw -Encoding UTF8))
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine()

    [void]$sb.AppendLine("## Граф зависимостей (dekko deps)")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine((Get-Content "docs\code-map\_deps.txt" -Raw -Encoding UTF8))
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine()

    [void]$sb.AppendLine("## Статистика (dekko stats)")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine((Get-Content "docs\code-map\_stats.txt" -Raw -Encoding UTF8))
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine()

    [void]$sb.AppendLine("## Граф вызовов (mermaid)")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine('```mermaid')
    [void]$sb.AppendLine((Get-Content "docs\code-map\callgraph.mmd" -Raw -Encoding UTF8))
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine()

    [void]$sb.AppendLine("## Карта проекта (первые 150 строк MAP.md)")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine('```markdown')
    $mapLines = Get-Content "docs\code-map\MAP.md" -TotalCount 150 -Encoding UTF8
    [void]$sb.AppendLine(($mapLines -join "`n"))
    [void]$sb.AppendLine('```')

    Set-Content -Path $outputFile -Value $sb.ToString() -Encoding UTF8

    $size = (Get-Item $outputFile).Length
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "  ГОТОВО: $outputFile" -ForegroundColor Green
    Write-Host "  Размер: $size байт" -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Green
}
finally {
    Pop-Location
}
