using System;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// Движок генерации человекоподобных таймингов на основе логнормального распределения.
/// Логнормальное распределение даёт "плотное ядро" быстрых реакций и длинный правый хвост
/// редких аномально долгих задержек — что соответствует реальной моторике человека.
/// Учитывает волновой множитель усталости (см. BotStateController).
/// </summary>
public static class HumanizerEngine
{
    private static readonly Random _random = new();

    // Жёсткие границы, чтобы экстремальные выбросы логнормальной кривой
    // не привели к зависанию макроса на секунды.
    private const int HardMinMs = 10;
    private const int HardMaxMs = 400;

    // Мягкий порог: значения выше него считаются "хвостовыми" и перегенерируются.
    // Это устраняет "wall effect" — плоскую стену на PDF-графике телеметрии.
    private const int SoftMaxMs = 320;

    // Сколько раз пытаться перегенерировать хвостовое значение, прежде чем
    // применить fallback на случайную величину у верхней границы.
    private const int MaxRerollAttempts = 5;

    /// <summary>
    /// Возвращает текущий волновой множитель усталости из контроллера состояния.
    /// 1.0 — бодрый, до ~1.4 — сильно уставший.
    /// </summary>
    private static double GetFatigueMultiplier() => BotStateController.FatigueMultiplier;

    /// <summary>
    /// Генерирует стандартную нормальную величину N(0,1) через преобразование Бокса-Мюллера.
    /// </summary>
    private static double NextGaussian()
    {
        // u1 в (0,1], чтобы избежать log(0)
        double u1 = 1.0 - _random.NextDouble();
        double u2 = 1.0 - _random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }

    /// <summary>
    /// Генерирует значение по логнормальному распределению.
    /// Параметры mu и sigma задаются в логарифмическом пространстве (натуральный логарифм миллисекунд).
    /// Усталость применяется и к mu (сдвиг среднего), и к sigma (удлинение хвоста).
    /// Вместо жёсткого clamp на верхней границе используется soft-cap с reroll,
    /// чтобы не создавать "wall effect" на PDF-графике телеметрии.
    /// </summary>
    private static int GetLogNormalDelay(double mu, double sigma)
    {
        double fatigue = GetFatigueMultiplier();

        // Сдвигаем среднее вверх и одновременно расширяем разброс.
        // Уставший игрок не только медленнее в среднем, но и чаще "залипает".
        double adjustedMu = mu + Math.Log(fatigue);
        double adjustedSigma = sigma * fatigue;

        double value = 0.0;

        // Soft-cap: перегенерируем хвостовые значения, чтобы не было плоской стены.
        for (int attempt = 0; attempt < MaxRerollAttempts; attempt++)
        {
            double z = NextGaussian();
            double logValue = adjustedMu + adjustedSigma * z;
            value = Math.Exp(logValue);

            if (value <= SoftMaxMs)
                break;
        }

        // Если после всех попыток значение всё ещё выше мягкого порога —
        // формируем плавный затухающий хвост вместо прямоугольного блока,
        // чтобы сохранить асимптотику логнормального распределения на PDF.
        if (value > SoftMaxMs)
        {
            value = SoftMaxMs + Math.Abs(NextGaussian() * 20.0);

            // Жёсткая граница для страховки от бесконечных зависаний макроса.
            if (value > HardMaxMs)
                value = HardMaxMs;
        }

        // Нижняя граница остаётся жёсткой — она физически осмысленна
        // (быстрее человеческого рефлекса быть нельзя).
        if (value < HardMinMs)
            value = HardMinMs;

        return (int)value;
    }

    /// <summary>Время физического удержания клавиши нажатой. Плотное ядро, умеренный хвост.</summary>
    public static int GetKeyPressDuration() => GetLogNormalDelay(mu: Math.Log(55.0), sigma: 0.28);

    /// <summary>
    /// Пауза перед нажатием следующей независимой кнопки (время переноса пальца).
    /// Короче по среднему, но с большим разбросом — палец может "промахнуться" и задержаться.
    /// </summary>
    public static int GetFlightTime() => GetLogNormalDelay(mu: Math.Log(95.0), sigma: 0.42);

    /// <summary>
    /// Пауза после зажатия модификатора (Shift/Ctrl/Alt) перед нажатием основной клавиши.
    /// Самый узкий разброс — это почти рефлекторное действие.
    /// </summary>
    public static int GetModifierDelay() => GetLogNormalDelay(mu: Math.Log(65.0), sigma: 0.22);
}
