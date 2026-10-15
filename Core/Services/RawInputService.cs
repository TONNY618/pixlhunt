using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PixelMacroEngine.Core.Input;

namespace PixelMacroEngine.Core.Services
{
    /// <summary>
    /// [Подсистема движка: Уровень 1 - Ввод]
    /// Глобальный перехват физического ввода через Raw Input API.
    /// Отсекает эмуляцию от платы Arduino и ведёт потокобезопасную таблицу
    /// состояний кнопок без вызовов WinAPI на каждом кадре.
    /// </summary>
    public static class RawInputService
    {
        // === Сообщения ===
        private const int WM_INPUT = 0x00FF;
        private const int WM_INPUT_DEVICE_CHANGE = 0x00FE;

        // === Флаги регистрации устройств ===
        private const uint RIDEV_INPUTSINK = 0x00000100;
        private const uint RIDEV_DEVNOTIFY = 0x00002000;

        // === Команды Raw Input API ===
        private const uint RID_INPUT = 0x10000003;
        private const uint RIDI_DEVICENAME = 0x20000007;

        // === Тип изменения списка устройств ===
        private const int GIDC_REMOVAL = 2;

        // === Типы устройств ===
        private const uint RIM_TYPEMOUSE = 0;
        private const uint RIM_TYPEKEYBOARD = 1;

        // === Флаги клавиатуры ===
        private const ushort RI_KEY_BREAK = 1;
        private const ushort RI_KEY_E0 = 2; // Расширенная клавиша (правые Ctrl/Alt/Win)

        // === Коды виртуальных клавиш модификаторов ===
        private const ushort VK_SHIFT = 0x10;
        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_MENU = 0x12; // Alt
        private const ushort VK_LSHIFT = 0xA0;
        private const ushort VK_RSHIFT = 0xA1;
        private const ushort VK_LCONTROL = 0xA2;
        private const ushort VK_RCONTROL = 0xA3;
        private const ushort VK_LMENU = 0xA4;
        private const ushort VK_RMENU = 0xA5;

        // === Флаги кнопок мыши ===
        private const ushort RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
        private const ushort RI_MOUSE_LEFT_BUTTON_UP = 0x0002;
        private const ushort RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0004;
        private const ushort RI_MOUSE_RIGHT_BUTTON_UP = 0x0008;
        private const ushort RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010;
        private const ushort RI_MOUSE_MIDDLE_BUTTON_UP = 0x0020;
        private const ushort RI_MOUSE_BUTTON_4_DOWN = 0x0040;
        private const ushort RI_MOUSE_BUTTON_4_UP   = 0x0080;
        private const ushort RI_MOUSE_BUTTON_5_DOWN = 0x0100;
        private const ushort RI_MOUSE_BUTTON_5_UP   = 0x0200;
        private const ushort RI_MOUSE_WHEEL         = 0x0400;
        private const ushort RI_MOUSE_HWHEEL        = 0x0800;

        // === Usages ===
        private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        private const ushort HID_USAGE_GENERIC_MOUSE = 0x02;
        private const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;

        // === Потокобезопасное состояние ===
        private static readonly ConcurrentDictionary<IntPtr, bool> _ignoredDevicesCache = new();
        private static readonly ConcurrentDictionary<ushort, bool> _keyStates = new();

        private static volatile bool _isLmbDown;
        private static volatile bool _isRmbDown;
        private static volatile bool _isMmbDown;
        private static volatile bool _isXButton1Down;
        private static volatile bool _isXButton2Down;

        // Временное отображение прокрутки колесика (для UI-монитора ввода).
        // Время храним в тиках UTC (long), чтобы корректно работать с Volatile.
        private static volatile string _lastWheelAction = string.Empty;
        private static long _lastWheelTicks = DateTime.MinValue.Ticks;

        /// <summary>Окно (в мс), в течение которого показывается последнее действие колесика.</summary>
        private const int WheelDisplayWindowMs = 350;

        // === Накопитель шагов колесика для триггеров (модель "таймаут бездействия") ===
        // Считаем количество щелчков в текущей серии отдельно для вверх/вниз.
        // Серия сбрасывается, если между соседними щелчками прошло больше
        // WhellTrigerReadIntervalMs (пользователь "тупил" дольше лимита).
        // Используем lock, так как операции "прочитать-очистить" должны быть атомарными.
        private static readonly object _wheelLock = new();
        private static DateTime _lastHWheelLeftTime = DateTime.MinValue;
        private static DateTime _lastHWheelRightTime = DateTime.MinValue;

        private static int _wheelUpCount = 0;
        private static DateTime _lastWheelUpTime = DateTime.MinValue;
        private static int _wheelDownCount = 0;
        private static DateTime _lastWheelDownTime = DateTime.MinValue;

        // Постоянный буфер для чтения Raw Input (избегаем AllocHGlobal/FreeHGlobal на каждое событие).
        // 128 байт с запасом покрывает RAWINPUT (48 байт на x64).
        private static readonly IntPtr _rawInputBuffer = Marshal.AllocHGlobal(128);

        private static HiddenMessageWindow? _window;
        private static bool _initialized;

        /// <summary>Успешно ли зарегистрирован низкоуровневый перехват ввода.</summary>
        public static bool IsRegisteredSuccessfully { get; private set; }

        /// <summary>Запускает глобальный перехват ввода. Идемпотентно.</summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            _window = new HiddenMessageWindow();
        }

        /// <summary>Сброс всех зафиксированных состояний клавиш и кнопок (при паузе/сбое).</summary>
        public static void ResetAllStates()
        {
            _isLmbDown = false;
            _isRmbDown = false;
            _isMmbDown = false;
            _isXButton1Down = false;
            _isXButton2Down = false;
            _keyStates.Clear();
        }

        public static bool IsKeyDown(ushort vKey)
        {
            return _keyStates.TryGetValue(vKey, out bool down) && down;
        }

        public static bool IsMouseButtonDown(int button)
        {
            return button switch
            {
                2 => _isRmbDown,
                3 => _isMmbDown,
                4 => _isXButton1Down,
                5 => _isXButton2Down,
                _ => _isLmbDown
            };
        }

        /// <summary>
        /// Возвращает таймаут бездействия (мс) между соседними щелчками колесика.
        /// Берётся из конфига (WhellTrigerReadIntervalMs); при некорректном значении — 1900 мс.
        /// </summary>
        private static int GetWheelInactivityTimeoutMs()
        {
            int timeoutMs = AppConfigService.Instance.WhellTrigerReadIntervalMs;
            return timeoutMs > 0 ? timeoutMs : 1900;
        }

        /// <summary>
        /// Регистрирует щелчки колесика в накопителе триггеров (модель "таймаут бездействия").
        /// </summary>
        /// <param name="up">Направление: true — вверх, false — вниз.</param>
        /// <param name="steps">
        /// Количество шагов в одном событии (обычно 1, но delta/120 может содержать сразу несколько).
        /// </param>
        private static void RecordWheelTick(bool up, int steps = 1)
        {
            if (steps < 1) steps = 1;

            int timeoutMs = GetWheelInactivityTimeoutMs();
            DateTime now = DateTime.UtcNow;

            lock (_wheelLock)
            {
                if (up)
                {
                    // Если пауза между щелчками превысила лимит — начинаем серию заново.
                    if (_lastWheelUpTime != DateTime.MinValue &&
                        (now - _lastWheelUpTime).TotalMilliseconds > timeoutMs)
                    {
                        _wheelUpCount = 0;
                    }

                    _wheelUpCount += steps;
                    _lastWheelUpTime = now; // перезапуск таймера ожидания следующего шага
                }
                else
                {
                    if (_lastWheelDownTime != DateTime.MinValue &&
                        (now - _lastWheelDownTime).TotalMilliseconds > timeoutMs)
                    {
                        _wheelDownCount = 0;
                    }

                    _wheelDownCount += steps;
                    _lastWheelDownTime = now;
                }
            }
        }

        /// <summary>
        /// Проверяет, накоплено ли не менее <paramref name="requiredSteps"/> щелчков колесика
        /// в заданном направлении в рамках текущей серии (модель "таймаут бездействия").
        /// При успехе счётчик этого направления сбрасывается.
        /// </summary>
        /// <param name="up">Направление: true — вверх, false — вниз.</param>
        /// <param name="requiredSteps">Порог срабатывания (число щелчков в серии).</param>
        /// <param name="windowMs">
        /// Не используется (оставлен для обратной совместимости сигнатуры).
        /// Таймаут всегда берётся из конфига (WhellTrigerReadIntervalMs).
        /// </param>
        public static bool ConsumeWheelTicks(bool up, int requiredSteps, int windowMs = 0)
        {
            if (requiredSteps < 1) requiredSteps = 1;

            int timeoutMs = GetWheelInactivityTimeoutMs();
            DateTime now = DateTime.UtcNow;

            lock (_wheelLock)
            {
                if (up)
                {
                    // Серия "остыла" — сбрасываем накопленное и не срабатываем.
                    if (_lastWheelUpTime != DateTime.MinValue &&
                        (now - _lastWheelUpTime).TotalMilliseconds > timeoutMs)
                    {
                        _wheelUpCount = 0;
                        return false;
                    }

                    if (_wheelUpCount >= requiredSteps)
                    {
                        _wheelUpCount = 0;
                        return true;
                    }
                    return false;
                }
                else
                {
                    if (_lastWheelDownTime != DateTime.MinValue &&
                        (now - _lastWheelDownTime).TotalMilliseconds > timeoutMs)
                    {
                        _wheelDownCount = 0;
                        return false;
                    }

                    if (_wheelDownCount >= requiredSteps)
                    {
                        _wheelDownCount = 0;
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Проверяет, был ли зафиксирован наклон горизонтального колесика (HWHEEL)
        /// в заданном направлении за последние <paramref name="windowMs"/> миллисекунд.
        /// При успехе метка времени сбрасывается (edge-trigger: одно событие — одно срабатывание).
        /// </summary>
        /// <param name="right">Направление: true — вправо (WR), false — влево (WL).</param>
        /// <param name="windowMs">Окно актуальности события в миллисекундах.</param>
        public static bool ConsumeHWheel(bool right, int windowMs = 350)
        {
            lock (_wheelLock)
            {
                var now = DateTime.UtcNow;
                if (right)
                {
                    if (_lastHWheelRightTime != DateTime.MinValue &&
                        (now - _lastHWheelRightTime).TotalMilliseconds <= windowMs)
                    {
                        _lastHWheelRightTime = DateTime.MinValue;
                        return true;
                    }
                }
                else
                {
                    if (_lastHWheelLeftTime != DateTime.MinValue &&
                        (now - _lastHWheelLeftTime).TotalMilliseconds <= windowMs)
                    {
                        _lastHWheelLeftTime = DateTime.MinValue;
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Возвращает человекочитаемое описание текущего физического действия мыши/клавиатуры.
        /// Используется UI-монитором ввода (заголовок groupBoxKeyMouseMonitor в редакторе комбо).
        /// </summary>
        public static string GetHumanReadableMouseAction()
        {
            // 1. Приоритет — недавняя прокрутка колесика (в пределах окна отображения)
            long lastTicks = Volatile.Read(ref _lastWheelTicks);
            if (lastTicks != DateTime.MinValue.Ticks)
            {
                double elapsedMs = (DateTime.UtcNow - new DateTime(lastTicks, DateTimeKind.Utc)).TotalMilliseconds;
                if (elapsedMs >= 0 && elapsedMs < WheelDisplayWindowMs)
                {
                    return _lastWheelAction;
                }
            }

            // 2. Зажатые кнопки мыши — собираем в список
            var parts = new List<string>(5);
            if (_isLmbDown) parts.Add("Левая кнопка (ЛКМ)");
            if (_isRmbDown) parts.Add("Правая кнопка (ПКМ)");
            if (_isMmbDown) parts.Add("Колесико (Клик / СКМ)");
            if (_isXButton1Down) parts.Add("Боковая кнопка (Назад)");
            if (_isXButton2Down) parts.Add("Боковая кнопка (Вперед)");

            if (parts.Count > 0)
                return string.Join(" + ", parts);

            // 3. Клавиатурные клавиши (многие мыши шлют макро-кнопки как клавиши).
            //    Переиспользуем таблицу ArduinoKeyMap.WindowsVkMap для перевода VK -> имя.
            foreach (var kvp in _keyStates)
            {
                if (kvp.Value)
                {
                    string name = ResolveKeyName(kvp.Key);
                    return $"KEY mouse/keyboard: {name}";
                }
            }

            // 4. Ничего не нажато
            return string.Empty;
        }

        /// <summary>
        /// Переводит Windows Virtual Key в человекочитаемое имя через ArduinoKeyMap.
        /// Если соответствие не найдено — возвращает строку с кодом.
        /// </summary>
        private static string ResolveKeyName(ushort vKey)
        {
            foreach (var kvp in ArduinoKeyMap.WindowsVkMap)
            {
                if (kvp.Value == vKey)
                    return kvp.Key;
            }
            return $"[Код {vKey}]";
        }

        /// <summary>
        /// Проверяет, произошло ли указанное действие колесика (например, "WL" или "WR")
        /// за последние windowMs миллисекунд. При успехе сбрасывает метку (edge-trigger).
        /// </summary>
        public static bool ConsumeWheelAction(string actionKey, int windowMs = 350)
        {
            long lastTicks = Volatile.Read(ref _lastWheelTicks);
            if (lastTicks == DateTime.MinValue.Ticks) return false;

            double elapsedMs = (DateTime.UtcNow - new DateTime(lastTicks, DateTimeKind.Utc)).TotalMilliseconds;
            if (elapsedMs >= 0 && elapsedMs <= windowMs)
            {
                if (_lastWheelAction.Contains(actionKey, StringComparison.OrdinalIgnoreCase))
                {
                    Volatile.Write(ref _lastWheelTicks, DateTime.MinValue.Ticks);
                    return true;
                }
            }
            return false;
        }

        // =====================================================================
        // СКРЫТОЕ ОКНО-ПРИЁМНИК RAW INPUT
        // =====================================================================
        private sealed class HiddenMessageWindow : NativeWindow
        {
            public HiddenMessageWindow()
            {
                // ВАЖНО: Не используем HWND_MESSAGE (-3), так как Windows блокирует 
                // широковещательный фоновый ввод (RIDEV_INPUTSINK) для чисто служебных окон.
                // Создаем невидимое окно верхнего уровня.
                var cp = new CreateParams
                {
                    Caption = "pxlhuntRawInputSink",
                    Style = 0, // Без WS_VISIBLE — окно невидимо
                    Parent = IntPtr.Zero
                };
                CreateHandle(cp);

                var devices = new RAWINPUTDEVICE[2];

                // Мышь
                devices[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
                devices[0].usUsage = HID_USAGE_GENERIC_MOUSE;
                devices[0].dwFlags = RIDEV_INPUTSINK | RIDEV_DEVNOTIFY;
                devices[0].hwndTarget = Handle;

                // Клавиатура
                devices[1].usUsagePage = HID_USAGE_PAGE_GENERIC;
                devices[1].usUsage = HID_USAGE_GENERIC_KEYBOARD;
                devices[1].dwFlags = RIDEV_INPUTSINK | RIDEV_DEVNOTIFY;
                devices[1].hwndTarget = Handle;

                IsRegisteredSuccessfully = RegisterRawInputDevices(devices, 2, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_INPUT_DEVICE_CHANGE:
                        if (m.WParam.ToInt32() == GIDC_REMOVAL)
                        {
                            _ignoredDevicesCache.TryRemove(m.LParam, out _);
                        }
                        break;

                    case WM_INPUT:
                        HandleInput(m.LParam);
                        break;
                }

                base.WndProc(ref m);
            }
        }

        // =====================================================================
        // ОБРАБОТКА КАДРА RAW INPUT
        // =====================================================================
        private static void HandleInput(IntPtr lParam)
        {
            uint headerSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();
            uint size = 0;

            // Обязательный запрос размера у Windows (Windows вернет 48 на x64)
            GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref size, headerSize);
            if (size == 0 || size > 128) return;

            // Читаем данные в наш заранее выделенный буфер _rawInputBuffer
            if (GetRawInputData(lParam, RID_INPUT, _rawInputBuffer, ref size, headerSize) != size)
                return;

            RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(_rawInputBuffer);

            // Фильтр эмулятора (Arduino)
            if (!_ignoredDevicesCache.TryGetValue(raw.header.hDevice, out bool ignore))
            {
                ignore = IsIgnoredDevice(raw.header.hDevice);
                _ignoredDevicesCache[raw.header.hDevice] = ignore;
            }
            if (ignore) return;

            if (raw.header.dwType == RIM_TYPEMOUSE)
            {
                ushort flags = raw.data.mouse.usButtonFlags;
                if (flags == 0) return; // Чистое перемещение мыши — выходим

                if ((flags & RI_MOUSE_LEFT_BUTTON_DOWN) != 0) _isLmbDown = true;
                if ((flags & RI_MOUSE_LEFT_BUTTON_UP) != 0) _isLmbDown = false;
                if ((flags & RI_MOUSE_RIGHT_BUTTON_DOWN) != 0) _isRmbDown = true;
                if ((flags & RI_MOUSE_RIGHT_BUTTON_UP) != 0) _isRmbDown = false;
                if ((flags & RI_MOUSE_MIDDLE_BUTTON_DOWN) != 0) _isMmbDown = true;
                if ((flags & RI_MOUSE_MIDDLE_BUTTON_UP) != 0) _isMmbDown = false;

                if ((flags & RI_MOUSE_BUTTON_4_DOWN) != 0) _isXButton1Down = true;
                if ((flags & RI_MOUSE_BUTTON_4_UP) != 0)   _isXButton1Down = false;
                if ((flags & RI_MOUSE_BUTTON_5_DOWN) != 0) _isXButton2Down = true;
                if ((flags & RI_MOUSE_BUTTON_5_UP) != 0)   _isXButton2Down = false;

                if ((flags & RI_MOUSE_WHEEL) != 0)
                {
                    short wheelDelta = (short)raw.data.mouse.usButtonData;
                    _lastWheelAction = wheelDelta > 0 ? "Колесико: Прокрутка вверх" : "Колесико: Прокрутка вниз";
                    Volatile.Write(ref _lastWheelTicks, DateTime.UtcNow.Ticks);

                    // Накопление шагов для триггеров (по направлению).
                    // delta/120 — число шагов в одном событии (обычно 1, но бывает и больше).
                    int steps = Math.Abs(wheelDelta) / 120;
                    if (steps < 1) steps = 1;
                    RecordWheelTick(wheelDelta > 0, steps);
                }
                else if ((flags & RI_MOUSE_HWHEEL) != 0)
                {
                    short hWheelDelta = (short)raw.data.mouse.usButtonData;
                    _lastWheelAction = hWheelDelta > 0 ? "Колесико: WR" : "Колесико: WL";
                    Volatile.Write(ref _lastWheelTicks, DateTime.UtcNow.Ticks);

                    lock (_wheelLock)
                    {
                        if (hWheelDelta > 0) _lastHWheelRightTime = DateTime.UtcNow;
                        else if (hWheelDelta < 0) _lastHWheelLeftTime = DateTime.UtcNow;
                    }
                }
            }
            else if (raw.header.dwType == RIM_TYPEKEYBOARD)
            {
                ushort vKey = raw.data.keyboard.VKey;
                if (vKey == 0 || vKey == 0xFFFF) return;

                ushort flags = raw.data.keyboard.Flags;
                bool isUp = (flags & RI_KEY_BREAK) != 0;

                // Конвертация универсальных модификаторов в конкретные L/R
                if (vKey == VK_SHIFT)
                {
                    vKey = (raw.data.keyboard.MakeCode == 0x36) ? VK_RSHIFT : VK_LSHIFT;
                }
                else if (vKey == VK_CONTROL)
                {
                    vKey = (flags & RI_KEY_E0) != 0 ? VK_RCONTROL : VK_LCONTROL;
                }
                else if (vKey == VK_MENU)
                {
                    vKey = (flags & RI_KEY_E0) != 0 ? VK_RMENU : VK_LMENU;
                }

                _keyStates[vKey] = !isUp;
            }
        }

        private static bool IsIgnoredDevice(IntPtr hDevice)
        {
            string? deviceName = GetDeviceName(hDevice);
            if (string.IsNullOrEmpty(deviceName)) return false;

            try
            {
                int vid = AppConfigService.Instance.Usb_hwKeyClicker.GetVendorId();
                int pid = AppConfigService.Instance.Usb_hwKeyClicker.GetProductId();

                if (vid == 0 && pid == 0) return false;

                // Основной формат: VID_XXXX&PID_XXXX
                string target = $"VID_{vid:X4}&PID_{pid:X4}";
                bool isMatch = deviceName.Contains(target, StringComparison.OrdinalIgnoreCase);

                // Альтернативные варианты записи (без ведущих нулей, с префиксом 0x и т.п.)
                if (!isMatch)
                {
                    string vidHex = vid.ToString("X");
                    string pidHex = pid.ToString("X");
                    string alt1 = $"VID_{vidHex}&PID_{pidHex}";
                    string alt2 = $"VID_{vid:X4}&PID_{pidHex}";
                    string alt3 = $"VID_{vidHex}&PID_{pid:X4}";

                    isMatch = deviceName.Contains(alt1, StringComparison.OrdinalIgnoreCase)
                           || deviceName.Contains(alt2, StringComparison.OrdinalIgnoreCase)
                           || deviceName.Contains(alt3, StringComparison.OrdinalIgnoreCase);
                }

                return isMatch;
            }
            catch
            {
                return false;
            }
        }

        private static string? GetDeviceName(IntPtr hDevice)
        {
            // ВАЖНО: GetRawInputDeviceInfoW с RIDI_DEVICENAME возвращает размер в СИМВОЛАХ (WCHAR),
            // а не в байтах. При повторном вызове нужно передавать именно это число символов.
            uint charCount = 0;
            uint res = GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, IntPtr.Zero, ref charCount);
            if (res == uint.MaxValue || charCount == 0) return null;

            // Выделяем память: charCount * sizeof(char)
            IntPtr buffer = Marshal.AllocHGlobal((int)charCount * 2);
            try
            {
                res = GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, buffer, ref charCount);
                if (res == uint.MaxValue) return null;
                return Marshal.PtrToStringUni(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        // =====================================================================
        // P/INVOKE И СТРУКТУРЫ
        // =====================================================================
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(
            RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputData(
            IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "GetRawInputDeviceInfoW")]
        private static extern uint GetRawInputDeviceInfo(
            IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWMOUSE
        {
            [FieldOffset(0)] public ushort usFlags;
            [FieldOffset(4)] public uint ulButtons;
            [FieldOffset(4)] public ushort usButtonFlags;
            [FieldOffset(6)] public ushort usButtonData;
            [FieldOffset(8)] public uint ulRawButtons;
            [FieldOffset(12)] public int lLastX;
            [FieldOffset(16)] public int lLastY;
            [FieldOffset(20)] public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWINPUTUNION
        {
            [FieldOffset(0)] public RAWMOUSE mouse;
            [FieldOffset(0)] public RAWKEYBOARD keyboard;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWINPUTUNION data;
        }
    }
}
