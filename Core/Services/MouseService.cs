using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelMacroEngine.Core.Input;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// Сервис управления мышью: человекоподобные клики, скролл и биомеханическое перемещение курсора
/// через Arduino RawHID. Использует TrajectoryPlanner (Фиттс, minimum jerk, тремор, овершут)
/// и PreciseTimer для стабилизации системного кванта таймера Windows.
/// </summary>
public static class MouseService
{
    private static readonly Random _random = new();

    // Состояние для авто-распознавания двойных кликов
    private static DateTime _lastClickTime = DateTime.MinValue;
    private static byte _lastClickButton = 0;
    private static readonly object _stateLock = new();

    static MouseService()
    {
        PreciseTimer.Initialize();
    }

    /// <summary>
    /// Человекоподобный клик мыши с авто-детекцией двойного клика и естественными задержками удержания.
    /// </summary>
    public static async Task ClickButtonAsync(byte button, bool pressDown, bool pressUp, string comboName, CancellationToken ct)
    {
        // Проверяем паттерн Double Click: если та же кнопка нажата быстрее чем за 380 мс
        bool isDoubleClick = false;
        lock (_stateLock)
        {
            var now = DateTime.UtcNow;
            if (_lastClickButton == button && (now - _lastClickTime).TotalMilliseconds < 380)
            {
                isDoubleClick = true;
            }
            _lastClickTime = now;
            _lastClickButton = button;
        }

        // Если это второй клик двойного нажатия — естественная межнажатийная пауза (80-160 мс)
        if (isDoubleClick && pressDown)
        {
            int interClickDelay = _random.Next(85, 155);
            await Task.Delay(interClickDelay, ct).ConfigureAwait(false);
        }

        if (pressDown)
        {
            var batch = new PacketBatch();
            batch.AddMouseButton(button, isDown: true);
            ArduinoHidService.Send(batch);
            ActionLogger.LogMouse($"Button {button} DOWN {(isDoubleClick ? "(Double-Click Part 2)" : "")}", comboName: comboName);
        }

        // Фаза удержания кнопки (нажатие пальцем: 60-110 мс)
        if (pressDown && pressUp)
        {
            int holdTime = _random.Next(55, 105);
            await Task.Delay(holdTime, ct).ConfigureAwait(false);
        }

        if (pressUp)
        {
            var batch = new PacketBatch();
            batch.AddMouseButton(button, isDown: false);
            ArduinoHidService.Send(batch);
            ActionLogger.LogMouse($"Button {button} UP", comboName: comboName);
        }

        // Пост-задержка после одиночного действия
        if (!isDoubleClick && ((pressDown && pressUp) || (!pressDown && pressUp)))
        {
            int flightTime = _random.Next(35, 75);
            await Task.Delay(flightTime, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Прокрутка колесика с нарастающими и затухающими интервалами между шагами.
    /// </summary>
    public static async Task ScrollAsync(int notches, bool scrollUp, string comboName, CancellationToken ct)
    {
        int count = Math.Clamp(notches, 1, 20);
        sbyte direction = scrollUp ? (sbyte)1 : (sbyte)-1;

        for (int i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var batch = new PacketBatch();
            batch.AddMouseMove(0, 0, direction);
            ArduinoHidService.Send(batch);

            // Интервалы вращения колеса пальцем (30-60 мс)
            await Task.Delay(_random.Next(28, 58), ct).ConfigureAwait(false);
        }

        ActionLogger.LogMouse($"Scroll {(scrollUp ? "UP" : "DOWN")} x{count}", comboName: comboName);
    }

    /// <summary>
    /// Биомеханическое перемещение мыши: реакция -> кинематика Фиттса -> буферизованный стриминг 500 Гц -> микрокоррекция -> фиксация.
    /// </summary>
    public static async Task MoveToAsync(int targetX, int targetY, int speedFactor, string comboName, CancellationToken ct)
    {
        Point start = Cursor.Position;
        Point target = new(targetX, targetY);

        ActionLogger.LogMouse($"Старт биомеханики к [{targetX}, {targetY}]", targetX, targetY);

        if (!ArduinoHidService.IsConnected)
        {
            ActionLogger.LogMouse("ОШИБКА: Arduino не подключена! Проверьте соединение.");
            return;
        }

        // --- Фаза 1. Латентный период реакции человека (110-230 мс) ---
        int reactionTime = _random.Next(110, 230);
        await Task.Delay(reactionTime, ct).ConfigureAwait(false);

        // --- Фаза 2. Расчет основной траектории (частота 500 Гц = шаг dt 0.002 сек) ---
        var deltas = TrajectoryPlanner.GenerateTrajectory(start, target, targetWidth: 24.0, dt: 0.002);

        try
        {
            // --- Фаза 3. Буферизованный стриминг пакетов ---
            await StreamDeltasToArduino(deltas, ct).ConfigureAwait(false);

            // --- Фаза 4. Зрительная обратная связь и микрокоррекция (Closed-Loop) ---
            // Если из-за акселерации Windows курсор не попал точно в цель, доводим его
            int correctionAttempts = 0;
            while (correctionAttempts < 5)
            {
                Point current = Cursor.Position;
                int errX = targetX - current.X;
                int errY = targetY - current.Y;

                // Если попали в цель с допуском 1 px — выходим
                if (Math.Abs(errX) <= 1 && Math.Abs(errY) <= 1)
                    break;

                correctionAttempts++;

                // Генерируем короткую доводку с маленькой шириной цели (осторожное движение)
                var correctionDeltas = TrajectoryPlanner.GenerateTrajectory(current, target, targetWidth: 4.0, dt: 0.002);
                await StreamDeltasToArduino(correctionDeltas, ct).ConfigureAwait(false);

                // Физиологическая пауза между микрокоррекциями (оценка глазами: 20-45 мс)
                await Task.Delay(_random.Next(20, 45), ct).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            ActionLogger.LogMouse($"ОШИБКА: {ex.Message}");
            return;
        }

        // --- Фаза 6. Предклик-фиксация (Ocular-Motor Fixation Pause) ---
        int fixationPause = _random.Next(45, 120);
        await Task.Delay(fixationPause, ct).ConfigureAwait(false);

        Point finalPos = Cursor.Position;
        int totalErr = Math.Abs(targetX - finalPos.X) + Math.Abs(targetY - finalPos.Y);
        ActionLogger.LogMouse($"Завершено в [{finalPos.X}, {finalPos.Y}] (Ошибка: {totalErr} px)", finalPos.X, finalPos.Y);
    }

    /// <summary>
    /// Упаковывает микро-движения по 16 штук (64 байта) и отправляет на Arduino,
    /// выдерживая паузу, пока плата аппаратно интерполирует их с задержкой 2 мс на шаг.
    /// </summary>
    private static async Task StreamDeltasToArduino(System.Collections.Generic.List<TrajectoryPlanner.StepDelta> deltas, CancellationToken ct)
    {
        var batch = new PacketBatch();
        int commandsInBatch = 0;

        foreach (var delta in deltas)
        {
            ct.ThrowIfCancellationRequested();

            batch.AddMouseMove(delta.Dx, delta.Dy);
            commandsInBatch++;

            // Как только пакет заполнен полностью (16 команд = 64 байта данных)
            if (commandsInBatch == 16)
            {
                if (!ArduinoHidService.Send(batch))
                    throw new Exception("Потеряна связь с Arduino во время движения.");

                // Arduino аппаратно проигрывает 16 шагов по 2 мс = 32 мс.
                // Ждем 28 мс, чтобы следующий пакет пришел ровно встык и буфер не опустел.
                await Task.Delay(28, ct).ConfigureAwait(false);

                // Очищаем батч для следующей партии
                batch = new PacketBatch();
                commandsInBatch = 0;
            }
        }

        // Если в конце остались неупакованные команды (меньше 16)
        if (commandsInBatch > 0)
        {
            if (!ArduinoHidService.Send(batch))
                throw new Exception("Потеряна связь с Arduino в конце движения.");

            // Ждем пропорционально количеству отправленных остатков
            await Task.Delay(commandsInBatch * 2, ct).ConfigureAwait(false);
        }
    }
}
