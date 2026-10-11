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
    /// Биомеханическое перемещение с хаотическим овершутом, реактивным отскоком и двухслойным микротремором.
    /// </summary>
    public static async Task MoveToAsync(int targetX, int targetY, int speedFactor, string comboName, CancellationToken ct)
    {
        Point start = Cursor.Position;
        ActionLogger.LogMouse($"Старт к [{targetX}, {targetY}] (скорость {speedFactor})", targetX, targetY);

        if (!ArduinoHidService.IsConnected) return;

        speedFactor = Math.Clamp(speedFactor, 1, 100);

        // Латентное время реакции: чем выше скорость/агрессия, тем быстрее старт
        int reactionTime = speedFactor > 70 ? _random.Next(25, 55) : _random.Next(50, 110);
        await Task.Delay(reactionTime, ct).ConfigureAwait(false);

        double totalDx = targetX - start.X;
        double totalDy = targetY - start.Y;
        double distance = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);
        if (distance < 2.0) return;

        double dirX = totalDx / distance;
        double dirY = totalDy / distance;
        double normX = -dirY;
        double normY = dirX;

        // --- Расчет длительности по Фиттсу с учетом speedFactor ---
        double a = _random.NextDouble() * 0.02 + 0.035;
        double b = _random.NextDouble() * 0.02 + 0.045;
        double id = Math.Log2((2.0 * distance / 32.0) + 1.0);
        double baseDurationMs = (a + b * id) * 1000.0;

        // Коэффициент ускорения: высокая скорость дает резкий бросок
        double speedModifier = 1.5 - (speedFactor / 100.0 * 1.05);
        double totalDurationMs = Math.Clamp(baseDurationMs * speedModifier, 130.0, 850.0);

        // --- Построение сплайна контрольных точек (Кэтмулл-Ром) ---
        var waypoints = new System.Collections.Generic.List<PointF>();
        int sign = _random.Next(2) == 0 ? 1 : -1;

        PointF ptStart = new(start.X, start.Y);
        PointF ptTarget = new(targetX, targetY);

        bool isHighSpeedScatter = speedFactor >= 60 && distance > 100.0;
        bool isLowSpeedWander = speedFactor <= 40;

        if (isHighSpeedScatter)
        {
            // РЕЖИМ ВЫСОКОЙ СКОРОСТИ: пролет насквозь в радиусе 40-90 px + судорожный отскок (чуть выше/вбок) + доводка
            double overshootDist = Math.Clamp(distance * (0.07 + speedFactor / 450.0), 35.0, 95.0);
            double lateralScatter = _random.Next(-45, 45); // Вынос в сторону

            // Точка овершута (пролета цели)
            PointF ptOvershoot = new(
                (float)(targetX + dirX * overshootDist + normX * lateralScatter),
                (float)(targetY + dirY * overshootDist + normY * lateralScatter)
            );

            // Реактивный отскок (судорожное движение назад, но со смещением "чуть выше/ниже")
            double hookLateral = -lateralScatter * (_random.NextDouble() * 0.5 + 0.3) + _random.Next(-18, 18);
            double hookLongitudinal = _random.Next(-15, 10);
            PointF ptHook = new(
                (float)(targetX + dirX * hookLongitudinal + normX * hookLateral),
                (float)(targetY + dirY * hookLongitudinal + normY * hookLateral)
            );

            // Промежуточный изгиб на старте (ломаная дуга)
            double midSwerve = distance * (_random.NextDouble() * 0.12 + 0.05) * sign;
            PointF ptMid = new(
                (float)(start.X + totalDx * 0.40 + normX * midSwerve),
                (float)(start.Y + totalDy * 0.40 + normY * midSwerve)
            );

            // Путь: Старт -> Изгиб -> Пролет цели -> Отскок -> Точная цель
            waypoints.Add(ptStart);
            waypoints.Add(ptMid);
            waypoints.Add(ptOvershoot);
            waypoints.Add(ptHook);
            waypoints.Add(ptTarget);
        }
        else if (isLowSpeedWander)
        {
            // РЕЖИМ МЕДЛЕННОЙ СКОРОСТИ: без проскоков, но больше волнообразного рыскания
            double w1 = distance * (_random.NextDouble() * 0.08 + 0.03) * sign;
            double w2 = distance * (_random.NextDouble() * 0.06 + 0.02) * -sign;

            waypoints.Add(ptStart);
            waypoints.Add(new PointF((float)(start.X + totalDx * 0.33 + normX * w1), (float)(start.Y + totalDy * 0.33 + normY * w1)));
            waypoints.Add(new PointF((float)(start.X + totalDx * 0.68 + normX * w2), (float)(start.Y + totalDy * 0.68 + normY * w2)));
            waypoints.Add(ptTarget);
        }
        else
        {
            // СРЕДНИЙ РЕЖИМ: умеренный перелет (15-25 px) с аккуратным возвратом
            double over = Math.Clamp(distance * 0.05, 12.0, 26.0);
            double side = _random.Next(-15, 15);
            waypoints.Add(ptStart);
            waypoints.Add(new PointF((float)(start.X + totalDx * 0.45 + normX * side * 0.7), (float)(start.Y + totalDy * 0.45 + normY * side * 0.7)));
            waypoints.Add(new PointF((float)(targetX + dirX * over + normX * side), (float)(targetY + dirY * over + normY * side)));
            waypoints.Add(ptTarget);
        }

        // Подготовка виртуальных точек сплайна Кэтмулла-Рома
        var splinePoints = new System.Collections.Generic.List<PointF>();
        PointF pFirst = waypoints[0];
        PointF pSecond = waypoints[1];
        PointF pLast = waypoints[^1];
        PointF pPrevLast = waypoints[^2];

        splinePoints.Add(new PointF(pFirst.X - (pSecond.X - pFirst.X), pFirst.Y - (pSecond.Y - pFirst.Y)));
        splinePoints.AddRange(waypoints);
        splinePoints.Add(new PointF(pLast.X + (pLast.X - pPrevLast.X), pLast.Y + (pLast.Y - pPrevLast.Y)));

        double elapsed = 0.0;
        double remainderX = 0.0;
        double remainderY = 0.0;

        // Амплитуда макротремора: при медленном ведении дрожь выше (2.5-4.5 px), при резком - мельче (1.2-2.2 px)
        double tremorBaseAmp = isLowSpeedWander ? (_random.NextDouble() * 1.5 + 2.5) : (_random.NextDouble() * 0.8 + 1.2);

        while (elapsed <= totalDurationMs && !ct.IsCancellationRequested)
        {
            double t = elapsed / totalDurationMs;
            if (t > 1.0) t = 1.0;

            // Асимметричный профиль скорости: быстрый рывок, плавный вход
            double u = Math.Pow(t, isHighSpeedScatter ? 0.75 : 0.85);
            double progress = 10 * Math.Pow(u, 3) - 15 * Math.Pow(u, 4) + 6 * Math.Pow(u, 5);

            // Вычисляем координату по непрерывному сплайну
            PointF splinePos = GetCatmullRomPoint(splinePoints, progress);

            // Двухосевой физиологический макротремор (8-12 Гц + низкочастотный дрейф)
            double envelope = Math.Sin(Math.PI * t); // На старте и в самом конце мягко сходит на нет
            double tremorX = (Math.Sin(elapsed * 0.065) * 0.7 + Math.Cos(elapsed * 0.023) * 0.3) * tremorBaseAmp * envelope;
            double tremorY = (Math.Cos(elapsed * 0.058) * 0.7 + Math.Sin(elapsed * 0.031) * 0.3) * tremorBaseAmp * envelope;

            double idealX = splinePos.X + tremorX;
            double idealY = splinePos.Y + tremorY;

            // Динамический замер ошибки экрана
            Point current = Cursor.Position;
            double neededX = idealX - current.X;
            double neededY = idealY - current.Y;

            var batch = new PacketBatch();
            double stepX = neededX / 16.0;
            double stepY = neededY / 16.0;

            for (int i = 0; i < 16; i++)
            {
                // Аппаратный микро-джиттер: случайные микроколебания ±1 px на отдельных шагах 500 Гц
                int jitterX = (i % 5 == 0) ? _random.Next(-1, 2) : 0;
                int jitterY = (i % 7 == 0) ? _random.Next(-1, 2) : 0;

                double exactX = stepX + remainderX + jitterX;
                double exactY = stepY + remainderY + jitterY;
                int intX = (int)Math.Round(exactX);
                int intY = (int)Math.Round(exactY);
                remainderX = exactX - intX;
                remainderY = exactY - intY;

                batch.AddMouseMove((sbyte)Math.Clamp(intX, -127, 127), (sbyte)Math.Clamp(intY, -127, 127));
            }

            if (!ArduinoHidService.Send(batch)) break;

            await Task.Delay(36, ct).ConfigureAwait(false);
            elapsed += 32.0;
        }

        // Короткая микрофиксация взгляда
        await Task.Delay(_random.Next(25, 50), ct).ConfigureAwait(false);
        Point finalPos = Cursor.Position;
        ActionLogger.LogMouse($"Завершено в [{finalPos.X}, {finalPos.Y}]", finalPos.X, finalPos.Y);
    }

    /// <summary>
    /// Вычисляет точку на сплайне Кэтмулла-Рома для заданного глобального прогресса t (0..1).
    /// </summary>
    private static PointF GetCatmullRomPoint(System.Collections.Generic.List<PointF> pts, double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        int sections = pts.Count - 3;
        double p = t * sections;
        int i = (int)p;
        if (i >= sections) i = sections - 1;

        double u = p - i;
        double u2 = u * u;
        double u3 = u2 * u;

        PointF p0 = pts[i];
        PointF p1 = pts[i + 1];
        PointF p2 = pts[i + 2];
        PointF p3 = pts[i + 3];

        float x = 0.5f * (float)((2 * p1.X) +
            (-p0.X + p2.X) * u +
            (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * u2 +
            (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * u3);

        float y = 0.5f * (float)((2 * p1.Y) +
            (-p0.Y + p2.Y) * u +
            (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * u2 +
            (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * u3);

        return new PointF(x, y);
    }
}
