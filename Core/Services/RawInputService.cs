using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

        private static HiddenMessageWindow? _window;
        private static bool _initialized;

        // === Логирование в файл ===
        private static readonly object _logLock = new();
        private static readonly string _logPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rawinput.log");

        /// <summary>Потокобезопасная запись строки в rawinput.log.</summary>
        private static void Log(string message)
        {
            try
            {
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [T{Environment.CurrentManagedThreadId}] {message}{Environment.NewLine}";
                lock (_logLock)
                {
                    File.AppendAllText(_logPath, line);
                }
            }
            catch
            {
                // Логирование не должно ломать работу сервиса
            }
        }

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
                _ => _isLmbDown
            };
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

                bool registered = RegisterRawInputDevices(devices, 2, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
                if (registered)
                {
                    Log("[RawInput] Регистрация устройств: УСПЕШНО");
                }
                else
                {
                    int err = Marshal.GetLastWin32Error();
                    Log($"[RawInput] Регистрация устройств: ОШИБКА Win32={err}");
                }
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

            GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref size, headerSize);
            if (size == 0) return;

            IntPtr buffer = Marshal.AllocHGlobal((int)size);
            try
            {
                if (GetRawInputData(lParam, RID_INPUT, buffer, ref size, headerSize) != size)
                    return;

                RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);

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

                    if ((flags & RI_MOUSE_LEFT_BUTTON_DOWN) != 0) { _isLmbDown = true; Log($"[Mouse] LMB DOWN flags=0x{flags:X4}"); }
                    if ((flags & RI_MOUSE_LEFT_BUTTON_UP) != 0) { _isLmbDown = false; Log($"[Mouse] LMB UP   flags=0x{flags:X4}"); }
                    if ((flags & RI_MOUSE_RIGHT_BUTTON_DOWN) != 0) { _isRmbDown = true; Log($"[Mouse] RMB DOWN flags=0x{flags:X4}"); }
                    if ((flags & RI_MOUSE_RIGHT_BUTTON_UP) != 0) { _isRmbDown = false; Log($"[Mouse] RMB UP   flags=0x{flags:X4}"); }
                    if ((flags & RI_MOUSE_MIDDLE_BUTTON_DOWN) != 0)
                    {
                        _isMmbDown = true;
                        Log($"[Mouse] MMB DOWN flags=0x{flags:X4}");
                    }
                    if ((flags & RI_MOUSE_MIDDLE_BUTTON_UP) != 0)
                    {
                        _isMmbDown = false;
                        Log($"[Mouse] MMB UP   flags=0x{flags:X4}");
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
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static bool IsIgnoredDevice(IntPtr hDevice)
        {
            string? deviceName = GetDeviceName(hDevice);
            if (string.IsNullOrEmpty(deviceName)) return false;

            Log($"[RawInput] Обнаружено устройство: {deviceName}");

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

                Log($"[RawInput] Проверка устройства: {deviceName} | Ищем VID: {vid:X4}, PID: {pid:X4} | Результат: {isMatch}");

                if (isMatch)
                {
                    Log($"[RawInput] ARDUINO IGNORED: {deviceName}");
                }

                return isMatch;
            }
            catch (Exception ex)
            {
                Log($"[RawInput] IsIgnoredDevice ошибка: {ex.Message}");
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
