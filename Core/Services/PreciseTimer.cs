using System;
using System.Runtime.InteropServices;

namespace PixelMacroEngine.Core.Services;

public static class PreciseTimer
{
    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    public static extern uint TimeBeginPeriod(uint uMilliseconds);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    public static extern uint TimeEndPeriod(uint uMilliseconds);

    static PreciseTimer()
    {
        // Устанавливаем системное разрешение таймера 1 мс на весь жизненный цикл приложения
        TimeBeginPeriod(1);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => TimeEndPeriod(1);
    }

    public static void Initialize() { }
}
