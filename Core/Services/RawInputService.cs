using System;
using System.Collections.Concurrent;
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
        // true = устройство принадлежит Arduino (эмулятору) и его ввод игнорируется.
        private static readonly ConcurrentDictionary<IntPtr, bool> _ignoredDevicesCache = new();
        // Карта: Virtual Key -> нажата ли физически.
        private static readonly ConcurrentDictionary<ushort, bool> _keyStates = new();

        private static volatile bool _isLmbDown;
        private static volatile bool _isRmbDown;
        private static volatile bool _isMmbDown;

        private static HiddenMessageWindow? _window;
        private static bool _initialized;

        /// <summary>Запускает глобальный перехват ввода. Идемпотентно.</summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            _window = new HiddenMessageWindow();
        }

        /// <summary>Нажата ли физическая клавиша с данным Virtual Key.</summary>
        public static bool IsKeyDown(ushort vKey)
        {
            return _keyStates.TryGetValue(vKey, out bool down) && down;
        }

        /// <summary>Нажата ли физическая кнопка мыши: 1 = ЛКМ, 2 = ПКМ, 3 = СКМ.</summary>
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
                var cp = new CreateParams
                {
                    Caption = "pxlhuntRawInputSink",
                    Parent = new IntPtr(-3), // HWND_MESSAGE — окно, невидимое для пользователя
                    Style = 0
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

                RegisterRawInputDevices(devices, 2, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_INPUT_DEVICE_CHANGE:
                        // Устройство физически отключено — выкидываем его из кэша,
                        // чтобы при повторном подключении выполнилась свежая проверка.
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

                // === Фильтр устройства (Arduino-эмулятор) ===
                if (!_ignoredDevicesCache.TryGetValue(raw.header.hDevice, out bool ignore))
                {
                    ignore = IsIgnoredDevice(raw.header.hDevice);
                    _ignoredDevicesCache[raw.header.hDevice] = ignore;
                }
                if (ignore) return;

                if (raw.header.dwType == RIM_TYPEMOUSE)
                {
                    // Защита от спама перемещений: чистое движение курсора не несёт кнопочных флагов.
                    if (raw.mouse.usButtonFlags == 0) return;

                    ushort flags = raw.mouse.usButtonFlags;

                    if ((flags & RI_MOUSE_LEFT_BUTTON_DOWN) != 0) _isLmbDown = true;
                    if ((flags & RI_MOUSE_LEFT_BUTTON_UP) != 0) _isLmbDown = false;
                    if ((flags & RI_MOUSE_RIGHT_BUTTON_DOWN) != 0) _isRmbDown = true;
                    if ((flags & RI_MOUSE_RIGHT_BUTTON_UP) != 0) _isRmbDown = false;
                    if ((flags & RI_MOUSE_MIDDLE_BUTTON_DOWN) != 0) _isMmbDown = true;
                    if ((flags & RI_MOUSE_MIDDLE_BUTTON_UP) != 0) _isMmbDown = false;
                }
                else if (raw.header.dwType == RIM_TYPEKEYBOARD)
                {
                    ushort vKey = raw.keyboard.VKey;
                    // 0/0xFFFF — служебные "фейковые" коды, игнорируем.
                    if (vKey == 0 || vKey == 0xFFFF) return;

                    bool isUp = (raw.keyboard.Flags & RI_KEY_BREAK) != 0;
                    _keyStates[vKey] = !isUp;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Возвращает true, если устройство принадлежит плате Arduino (по VID/PID из конфига).
        /// </summary>
        private static bool IsIgnoredDevice(IntPtr hDevice)
        {
            string? deviceName = GetDeviceName(hDevice);
            if (string.IsNullOrEmpty(deviceName)) return false;

            try
            {
                int vid = AppConfigService.Instance.Usb_hwKeyClicker.GetVendorId();
                int pid = AppConfigService.Instance.Usb_hwKeyClicker.GetProductId();

                if (vid == 0 && pid == 0) return false;

                string target = $"VID_{vid:X4}&PID_{pid:X4}";
                return deviceName!.Contains(target, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // При любой ошибке конфига не блокируем ввод.
                return false;
            }
        }

        /// <summary>Читает системное имя устройства (в нём содержатся VID/PID).</summary>
        private static string? GetDeviceName(IntPtr hDevice)
        {
            uint size = 0;
            uint res = GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, IntPtr.Zero, ref size);
            if (res == uint.MaxValue || size == 0) return null;

            IntPtr buffer = Marshal.AllocHGlobal((int)size * 2); // WCHAR = 2 байта
            try
            {
                res = GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, buffer, ref size);
                if (res == uint.MaxValue) return null;
                return Marshal.PtrToStringUni(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        // =====================================================================
        // P/INVOKE
        // =====================================================================
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(
            RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputData(
            IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputDeviceInfo(
            IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);

        // =====================================================================
        // СТРУКТУРЫ RAW INPUT
        // =====================================================================
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

        // Объединение payload мыши/клавиатуры.
        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWINPUTUNION
        {
            [FieldOffset(0)] public RAWMOUSE mouse;
            [FieldOffset(0)] public RAWKEYBOARD keyboard;
        }

        // Заголовок + union. При Sequential смещение union естественно совпадает
        // и на x86 (16), и на x64 (24).
        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWINPUTUNION data;
        }
    }
}
