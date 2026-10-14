using System;
using System.Diagnostics;

namespace PixelMacroEngine.Core.Services
{
    /// <summary>
    /// Единая точка работы со временем для всей системы.
    ///
    /// Класс намеренно разделяет два принципиально разных сценария:
    ///
    /// 1) Логирование и пользовательские метки — это "человеческое" мировое время.
    ///    Здесь нужен <see cref="DateTime"/>, потому что важно абсолютное значение
    ///    даты/времени (когда именно произошло событие), а не интервалы.
    ///    Использование UtcNow исключает скачки при переходе на летнее время и
    ///    зависит от системных часов, которые могут корректироваться (NTP, ручная
    ///    настройка). Поэтому <see cref="DateTime"/> НИКОГДА нельзя использовать
    ///    для измерения длительности.
    ///
    /// 2) Высокоточное измерение интервалов и задержек — это <see cref="Stopwatch"/>.
    ///    Его таймстампы берутся из аппаратного счётчика с высоким разрешением и
    ///    не зависят от изменений системных часов. Здесь важна только РАЗНИЦА между
    ///    двумя счётчиками (start/end), а не абсолютное значение.
    ///
    /// Все методы статические, не сохраняют состояние и не аллоцируют память в
    /// горячих путях, поэтому класс поточно-безопасен по построению (нет общего
    /// изменяемого состояния).
    /// </summary>
    public static class TimeService
    {
        // --------------------------------------------------------------------
        // 1. Логирование: реальное мировое время (DateTime)
        // --------------------------------------------------------------------

        /// <summary>
        /// Текущее время в UTC с высокой точностью (обычно ~100 нс на Windows).
        /// Используйте для меток времени в логах, чтобы записи были однозначны
        /// и не зависели от часового пояса и перехода на летнее время.
        ///
        /// ВНИМАНИЕ: не используйте UtcNow для измерения длительности операций —
        /// системные часы могут подстраиваться (NTP), что приведёт к искажению
        /// интервала (вплоть до отрицательных значений).
        /// </summary>
        public static DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        /// Текущее локальное время. Нужно только для отображения пользователю;
        /// в логах предпочтительнее <see cref="UtcNow"/>.
        /// </summary>
        public static DateTime Now => DateTime.Now;

        /// <summary>
        /// Готовый формат лог-штампа: "yyyy-MM-dd HH:mm:ss.fff".
        /// Например: "2026-10-14 13:05:42.317".
        /// </summary>
        public const string LogTimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// Форматированная метка времени для логов (по UTC).
        /// Пример результата: "2026-10-14 13:05:42.317".
        /// </summary>
        public static string GetLogTimestamp()
            => DateTime.UtcNow.ToString(LogTimestampFormat, System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Форматированная метка времени для логов по локальному времени.
        /// Используйте, если запись должна соответствовать часам пользователя.
        /// </summary>
        public static string GetLocalLogTimestamp()
            => DateTime.Now.ToString(LogTimestampFormat, System.Globalization.CultureInfo.InvariantCulture);

        // --------------------------------------------------------------------
        // 2. Высокоточное измерение интервалов (Stopwatch)
        // --------------------------------------------------------------------

        /// <summary>
        /// Частота аппаратного счётчика <see cref="Stopwatch"/> в тиках за секунду.
        /// Обычно 10 000 000 (100 нс), но значение платформозависимо и его нельзя
        /// считать константой. Берётся один раз при первом обращении (ленивая
        /// инициализация, статическое поле после этого — только чтение).
        /// </summary>
        public static long TimestampFrequency => Stopwatch.Frequency;

        /// <summary>
        /// Высокоразрешающий аппаратный таймстамп.
        /// Используйте пару start/end и передавайте их в методы ниже для
        /// измерения интервалов. Значение само по себе бессмысленно —
        /// важна только разница.
        /// </summary>
        public static long GetTimestamp() => Stopwatch.GetTimestamp();

        /// <summary>
        /// Разница между текущим моментом и таймстампом <paramref name="startingTimestamp"/>
        /// в виде <see cref="TimeSpan"/>.
        /// Использует Stopwatch.GetElapsedTime (.NET 7+) — без аллокаций и без
        /// пересчёта вручную через частоту счётчика.
        /// </summary>
        public static TimeSpan GetElapsedTime(long startingTimestamp)
            => Stopwatch.GetElapsedTime(startingTimestamp);

        /// <summary>
        /// Разница между двумя таймстампами (start → end) в виде <see cref="TimeSpan"/>.
        /// </summary>
        public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
            => Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);

        /// <summary>
        /// Количество миллисекунд, прошедших с момента <paramref name="startingTimestamp"/>.
        /// Возвращает double, чтобы сохранить доли миллисекунд (важно для точных задержек).
        /// </summary>
        public static double GetElapsedMilliseconds(long startingTimestamp)
            => Stopwatch.GetElapsedTime(startingTimestamp).TotalMilliseconds;

        /// <summary>
        /// Количество миллисекунд между двумя таймстампами (start → end).
        /// </summary>
        public static double GetElapsedMilliseconds(long startingTimestamp, long endingTimestamp)
            => Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp).TotalMilliseconds;

        /// <summary>
        /// Признак того, что используемый таймер основан на аппаратном счётчике
        /// с высоким разрешением (а не на служебных тиках планировщика).
        /// Полезно один раз залогировать при старте приложения, чтобы убедиться,
        /// что точность задержек максимальна.
        /// </summary>
        public static bool IsHighResolution => Stopwatch.IsHighResolution;
    }
}
