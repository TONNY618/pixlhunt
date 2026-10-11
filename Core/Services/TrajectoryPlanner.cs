using System;
using System.Collections.Generic;
using System.Drawing;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// Генератор кинематики траектории мыши на основе законов моторного контроля человека:
/// Закон Фиттса, полином Flash-Hogan (minimum jerk), физиологический тремор (8-12 Гц),
/// низкочастотный дрейф (1-3 Гц) и баллистический овершут с микрокоррекцией.
/// </summary>
public static class TrajectoryPlanner
{
    private static readonly Random _rng = new();

    public record StepDelta(sbyte Dx, sbyte Dy);

    /// <summary>
    /// Генерирует последовательность относительных смещений с шагом дискретизации dt (по умолч. 2 мс = 500 Гц).
    /// </summary>
    public static List<StepDelta> GenerateTrajectory(Point start, Point target, double targetWidth = 24.0, double dt = 0.002)
    {
        var deltas = new List<StepDelta>();

        double totalDx = target.X - start.X;
        double totalDy = target.Y - start.Y;
        double totalDistance = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);

        if (totalDistance < 1.0)
            return deltas;

        // --- Фаза 2: Расчет времени движения по закону Фиттса (Fitts' Law) ---
        // MT = a + b * log2(2D / W)
        double a = RandomRange(0.09, 0.14); // 90-140 мс базовая задержка
        double b = RandomRange(0.12, 0.17); // 120-170 мс/бит пропускная способность нервной системы
        double indexOfDifficulty = Math.Log2((2.0 * totalDistance / Math.Max(4.0, targetWidth)) + 1.0);
        double totalDuration = Math.Clamp(a + b * indexOfDifficulty, 0.18, 1.20); // общее время от 180 до 1200 мс

        // --- Фаза 3: Моделирование овершута (перелета) ---
        // На дистанциях > 150 px с вероятностью 45% человек слегка проскакивает цель
        bool willOvershoot = totalDistance > 150.0 && _rng.NextDouble() < 0.45;
        double overshootMagnitude = willOvershoot ? RandomRange(0.02, 0.05) * totalDistance : 0.0;

        // Направление движения
        double dirX = totalDx / totalDistance;
        double dirY = totalDy / totalDistance;
        double normX = -dirY; // Перпендикулярный вектор для кривизны
        double normY = dirX;

        // Кривизна траектории (1-3% от дистанции со случайным знаком)
        double curveAmp = RandomRange(0.01, 0.03) * totalDistance * (_rng.Next(2) == 0 ? 1 : -1);

        // Параметры тремора
        double tremorFreq = RandomRange(8.5, 11.5);   // 8-12 Гц физиологический тремор
        double tremorAmp = RandomRange(0.4, 1.2);      // амплитуда 0.4-1.2 px
        double driftFreq = RandomRange(1.2, 2.5);     // 1-3 Гц низкочастотный дрейф
        double driftAmp = RandomRange(1.0, 2.2);

        double tremorPhase = _rng.NextDouble() * Math.PI * 2;
        double driftPhase = _rng.NextDouble() * Math.PI * 2;

        int totalTicks = (int)Math.Max(10, Math.Round(totalDuration / dt));

        double lastPosX = start.X;
        double lastPosY = start.Y;

        // Накопитель дробной части для устранения дрейфа округления
        double remainderX = 0.0;
        double remainderY = 0.0;

        for (int i = 1; i <= totalTicks; i++)
        {
            double t = (double)i / totalTicks; // [0..1]

            // Асимметричный Minimum Jerk: рука тормозит дольше, чем разгоняется (пик скорости смещен на ~38%)
            double u = Math.Pow(t, 0.88);
            double progress = 10 * Math.Pow(u, 3) - 15 * Math.Pow(u, 4) + 6 * Math.Pow(u, 5);

            // Дистанция с учетом перелета: в районе t=0.75-0.85 выходим в легкий овершут, затем плавно возвращаемся
            double overshootOffset = 0.0;
            if (willOvershoot)
            {
                // Колоколообразное возмущение овершута в последней трети траектории
                overshootOffset = overshootMagnitude * Math.Sin(Math.PI * Math.Pow(t, 2.0));
            }

            double currentDist = (totalDistance * progress) + overshootOffset;

            // Базовая координата по вектору
            double posX = start.X + (dirX * currentDist);
            double posY = start.Y + (dirY * currentDist);

            // Перпендикулярная дуга (синусоида)
            double curveOffset = curveAmp * Math.Sin(Math.PI * t);
            posX += normX * curveOffset;
            posY += normY * curveOffset;

            // Фаза 5: Тремор и микрошум (затухают ближе к полной остановке)
            double dampening = Math.Sin(Math.PI * t); // На краях тремор слабее, в середине ярче
            double tremor = Math.Sin((2 * Math.PI * tremorFreq * (i * dt)) + tremorPhase) * tremorAmp * dampening;
            double drift = Math.Sin((2 * Math.PI * driftFreq * (i * dt)) + driftPhase) * driftAmp * dampening;

            posX += (normX * (tremor + drift));
            posY += (normY * (tremor + drift));

            // Расчет дельты текущего шага
            double stepDx = (posX - lastPosX) + remainderX;
            double stepDy = (posY - lastPosY) + remainderY;

            int intDx = (int)Math.Round(stepDx);
            int intDy = (int)Math.Round(stepDy);

            remainderX = stepDx - intDx;
            remainderY = stepDy - intDy;

            lastPosX = posX;
            lastPosY = posY;

            // Клампинг под формат sbyte [-127..127] с разбиением при резких рывках
            SplitAndAppend(deltas, intDx, intDy);
        }

        // Гарантированная финальная микрокоррекция (устраняет любые остатки до идеального таргета)
        int finalErrX = (int)Math.Round(target.X - lastPosX + remainderX);
        int finalErrY = (int)Math.Round(target.Y - lastPosY + remainderY);
        if (finalErrX != 0 || finalErrY != 0)
        {
            SplitAndAppend(deltas, finalErrX, finalErrY);
        }

        return deltas;
    }

    private static void SplitAndAppend(List<StepDelta> list, int dx, int dy)
    {
        while (Math.Abs(dx) > 120 || Math.Abs(dy) > 120)
        {
            sbyte chunkX = (sbyte)Math.Clamp(dx, -120, 120);
            sbyte chunkY = (sbyte)Math.Clamp(dy, -120, 120);
            list.Add(new StepDelta(chunkX, chunkY));
            dx -= chunkX;
            dy -= chunkY;
        }

        // ВАЖНО: Всегда добавляем финальный шаг, даже если это (0, 0).
        // Это заставляет Arduino ждать 2 мс, сохраняя плавность человеческой кривой.
        list.Add(new StepDelta((sbyte)dx, (sbyte)dy));
    }

    private static double RandomRange(double min, double max) => min + (_rng.NextDouble() * (max - min));
}
