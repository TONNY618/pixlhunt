using System;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// Движок генерации человекоподобных таймингов (Log-Normal / Ex-Gaussian аппроксимация).
/// Учитывает множитель усталости на основе длительности текущей сессии.
/// </summary>
public static class HumanizerEngine
{
    private static readonly Random _random = new();

    /// <summary>
    /// Рассчитывает множитель усталости.
    /// 0 минут = 1.0 (бодрый). 60 минут = 1.15. 120 минут = 1.30 (потолок).
    /// </summary>
    private static double GetFatigueMultiplier()
    {
        double minutes = BotStateController.ActiveSessionTime.TotalMinutes;
        if (minutes <= 0) return 1.0;

        // Линейный рост усталости до 2 часов (120 минут)
        double fatigue = 1.0 + (minutes / 120.0) * 0.30;
        return Math.Min(fatigue, 1.30);
    }

    /// <summary>
    /// Генерирует задержку с Гауссовым (нормальным) распределением и "длинным хвостом" вправо.
    /// </summary>
    private static int GetHumanDelay(int min, int mean, int max)
    {
        // Преобразование Бокса-Мюллера для Гауссова распределения
        double u1 = 1.0 - _random.NextDouble();
        double u2 = 1.0 - _random.NextDouble();
        double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        double fatigue = GetFatigueMultiplier();
        double adjustedMean = mean * fatigue;
        double adjustedMax = max * fatigue;

        double stdDev = (adjustedMean - min) / 2.0;
        double result = adjustedMean + (z * stdDev);

        // Имитация микро-затупа: если Гаусс уходит ниже физического минимума,
        // "отражаем" его далеко вправо (длинный хвост лог-нормального распределения)
        if (result < min)
        {
            result = adjustedMean + Math.Abs(z * stdDev * 1.5);
        }

        return (int)Math.Clamp(result, min, adjustedMax);
    }

    /// <summary>Время физического удержания клавиши нажатой.</summary>
    public static int GetKeyPressDuration() => GetHumanDelay(35, 55, 120);

    /// <summary>Пауза перед нажатием следующей независимой кнопки (время переноса пальца).</summary>
    public static int GetFlightTime() => GetHumanDelay(60, 110, 250);

    /// <summary>Пауза после зажатия модификатора (Shift/Ctrl/Alt) перед нажатием основной клавиши.</summary>
    public static int GetModifierDelay() => GetHumanDelay(40, 70, 150);
}
