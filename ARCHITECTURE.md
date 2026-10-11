# Архитектура проекта PixelMacroEngine (pxlhunt)

PixelMacroEngine — высокопроизводительный движок на C# (.NET / WinForms) для автоматизации действий по экранным триггерам с аппаратной эмуляцией ввода через физический микроконтроллер (Arduino Leonardo / Pro Micro на чипе ATmega32u4) и обходом поведенческих систем защиты (античитов) за счёт физиологической модели усталости и логнормальной рандомизации задержек.

---

## 1. Ключевые компоненты и подсистемы

### 1. Захват экрана и анализ (`Core\Capture`, `Core\Models\FrameBuffer`)
- **`DxgiScreenCapture`**: Низкоуровневый захват экрана с использованием Windows Desktop Duplication API (DirectX / DXGI). Обеспечивает минимальную задержку и оверхед по CPU/GPU.
- **`FrameBuffer`**: Абстракция кадра, предоставляющая доступ к пикселям кадра (`GetPixelColor(x, y)`), расчету средних цветов зон (`GetAverageColor`) и валидации условий.

### 2. Триггеры (`ITriggerEvaluator`, `Triggers\*`)
Каждый триггер реализует интерфейс `ITriggerEvaluator`:
- `bool Evaluate(FrameBuffer buffer)`: проверка выполнения условия на текущем кадре.
- **Реализации:**
  - `ColorTriggerEvaluator`: проверка цвета конкретного пикселя или диапазона/допуска цвета в координатах кадра.
  - `KeyTriggerEvaluator`: проверка состояния виртуальных клавиш.
  - `TimerTriggerEvaluator`: проверка истечения заданного интервала времени.

### 3. Шаги выполнения комбо (`IComboStep`, `Triggers\Steps\*`)
Каждый шаг реализует интерфейс `IComboStep`:
- `Task ExecuteAsync(TriggerExecutionContext context, CancellationToken ct)`
- **`TriggerExecutionContext`**: Передаёт текущий `FrameBuffer`, ссылку на активный триггер, его вес и имя запущенного комбо (`ComboName`).
- **Реализации:**
  - `PressKeyStep`: генерация нажатия/отпускания клавиши или последовательности клавиш через микроконтроллер с хуманизированными таймингами.
  - `DelayStep`: асинхронное ожидание с рандомизацией.
  - `AwaitStep`: ожидание наступления определённого экранного условия перед продолжением.
  - `ConditionStep`: ветвление логики комбо на основе повторной валидации условий.

### 4. Оркестратор и модели исполнения (`Engine\Orchestrator`, `Core\Models\ActiveCombo`)
- **`ActiveCombo`**: Модель комбо, содержащая:
  - Список триггеров (`Triggers`) и логику их сопоставления (`IsOrTriggerLogic` — ИЛИ / И).
  - Список последовательных шагов (`Steps`).
  - Приоритет выполнения (`Priority`).
  - Параметры кулдауна (`CooldownMs`, `CanExecute()`, `LastExecuted`).
  - Флаг `PauseIfWasd`: приостановка/пропуск комбо при удержании клавиш движения игрока (W, A, S, D).
  - Флаг выполнения (`IsRunning`) для предотвращения параллельного перезапуска одного и того же комбо.
- **`Orchestrator`**:
  - `ProcessFrameAsync(FrameBuffer frame, CancellationToken ct)`:
    - Вычисляет точный `DeltaTime` между кадрами через независимый `Stopwatch`.
    - Сортирует комбо по убыванию приоритета.
    - Проверяет WASD через `GetAsyncKeyState`.
    - Опрашивает триггеры и асинхронно исполняет шаги подходящего комбо с контролем отмены и паузы на движение.
    - Маппит активность на физиологическое состояние игрока (`BotStateController.PhysiologicalState`: `Action`, `Navigation`, `TrueIdle`).

### 5. Аппаратный ввод и хуманизация (`Core\Input`, `Core\Services`)
- **`ArduinoHidService`**: Обмен с контроллером через виртуальный COM-порт (CDC).
- **`PacketBatch`**: Формирование бинарных пакетов HID-команд (перемещение мыши, нажатия кнопок мыши, клавиатурные события).
- **`ArduinoKeyMap`**: Таблица трансляции Win32 Virtual Key (VK) в скан-коды USB HID Keyboard.
- **`HumanizerEngine`**: Генератор задержек на базе логнормального и гауссова распределений (press duration, flight time, modifier key down/up delay).
- **`BotStateController`**: Управляет физиологической усталостью бота (увеличивает разброс и среднее время отклика при долгой непрерывной активности).

---

## 2. UML-диаграмма классов

