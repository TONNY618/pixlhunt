using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HidSharp;

namespace pxlhunt
{
    public enum ArduinoKey : byte
    {
        // ==========================================
        // 1. МОДИФИКАТОРЫ
        // ==========================================
        LeftCtrl = 0x80,
        LeftShift = 0x81,
        LeftAlt = 0x82,
        LeftGui = 0x83, // Левая клавиша Windows
        RightCtrl = 0x84,
        RightShift = 0x85,
        RightAlt = 0x86, // AltGr
        RightGui = 0x87, // Правая клавиша Windows

        // ==========================================
        // 2. СИСТЕМНЫЕ И СЛУЖЕБНЫЕ КЛАВИШИ
        // ==========================================
        Enter = 0xB0,
        Escape = 0xB1,
        Backspace = 0xB2,
        Tab = 0xB3,
        Space = 0x20, // Пробел
        CapsLock = 0xC1,
        PrintScreen = 0xCE,
        ScrollLock = 0xCF,
        Pause = 0xD0,

        // ==========================================
        // 3. БЛОК НАВИГАЦИИ И СТРЕЛКИ
        // ==========================================
        Insert = 0xD1,
        Home = 0xD2,
        PageUp = 0xD3,
        Delete = 0xD4,
        End = 0xD5,
        PageDown = 0xD6,
        RightArrow = 0xD7,
        LeftArrow = 0xD8,
        DownArrow = 0xD9,
        UpArrow = 0xDA,

        // ==========================================
        // 4. ФУНКЦИОНАЛЬНЫЕ КЛАВИШИ (F1 - F12)
        // ==========================================
        F1 = 0xC2, F2 = 0xC3, F3 = 0xC4, F4 = 0xC5,
        F5 = 0xC6, F6 = 0xC7, F7 = 0xC8, F8 = 0xC9,
        F9 = 0xCA, F10 = 0xCB, F11 = 0xCC, F12 = 0xCD,

        // ==========================================
        // 5. ЦИФРОВОЙ БЛОК (NUMPAD)
        // ==========================================
        NumLock = 0xDB,
        NumPadSlash = 0xDC, // NumPad /
        NumPadStar = 0xDD, // NumPad *
        NumPadMinus = 0xDE, // NumPad -
        NumPadPlus = 0xDF, // NumPad +
        NumPadEnter = 0xE0, // NumPad Enter
        NumPad1 = 0xE1,
        NumPad2 = 0xE2,
        NumPad3 = 0xE3,
        NumPad4 = 0xE4,
        NumPad5 = 0xE5,
        NumPad6 = 0xE6,
        NumPad7 = 0xE7,
        NumPad8 = 0xE8,
        NumPad9 = 0xE9,
        NumPad0 = 0xEA,
        NumPadDot = 0xEB, // NumPad . (точка)

        // ==========================================
        // 6. СТАНДАРТНЫЕ ЦИФРЫ ВЕРХНЕГО РЯДА (Для справки)
        // ==========================================
        D0 = (byte)'0', D1 = (byte)'1', D2 = (byte)'2', D3 = (byte)'3', D4 = (byte)'4',
        D5 = (byte)'5', D6 = (byte)'6', D7 = (byte)'7', D8 = (byte)'8', D9 = (byte)'9'
    }

    public partial class testArduino : Form
    {
        // Список известных профилей платы (Chicony на первом месте)
        private readonly (int Vid, int Pid, string Name)[] _knownTargets = new[]
        {
            (0x04F2, 0x0833, "Chicony Wireless Device (0x04F2 : 0x0833)"),
            (0x2341, 0x8036, "Arduino Leonardo (0x2341 : 0x8036)")
        };

        private HidDevice _hidDevice = null;
        private HidStream _hidStream = null;
        private readonly Random _random = new Random();

        public testArduino()
        {
            InitializeComponent();

            // Привязка обработчиков событий к кнопкам
            SendKeybCMD.Click += SendKeybCMD_Click;
            SendMouseCMD.Click += SendMouseCMD_Click;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // Ограничитель значений (совместим со всеми версиями .NET)
        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // =========================================================
        // 1. БАЗОВЫЙ ТРАНСПОРТ HID
        // =========================================================

        private bool EnsureConnected()
        {
            if (_hidStream != null) return true;

            var loader = new HidDeviceLoader();
            foreach (var target in _knownTargets)
            {
                var devices = loader.GetDevices(target.Vid, target.Pid).ToList();

                foreach (var dev in devices)
                {
                    if (dev.TryOpen(out var stream))
                    {
                        if (dev.MaxOutputReportLength >= 64)
                        {
                            _hidDevice = dev;
                            _hidStream = stream;
                            return true;
                        }

                        stream.Dispose();
                    }
                }
            }

            MessageBox.Show(
                "Не удалось подключиться к плате.\nПроверьте, вставлен ли кабель USB.",
                "Ошибка подключения",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        private void SendRawCommand(byte cmdType, byte p1 = 0, byte p2 = 0, byte p3 = 0)
        {
            if (!EnsureConnected()) return;

            try
            {
                byte[] packet = new byte[_hidDevice.MaxOutputReportLength];
                packet[0] = 0x00; // Report ID
                packet[1] = cmdType;
                packet[2] = p1;
                packet[3] = p2;
                packet[4] = p3;

                lock (_hidStream)
                {
                    _hidStream.Write(packet);
                }
            }
            catch
            {
                _hidStream?.Dispose();
                _hidStream = null;
                _hidDevice = null;
            }
        }

        // =========================================================
        // 2. УПРАВЛЕНИЕ КЛАВИАТУРОЙ
        // =========================================================

        public void SendKeyDown(byte keyCode) => SendRawCommand(0x03, keyCode, 1);
        public void SendKeyUp(byte keyCode) => SendRawCommand(0x03, keyCode, 0);

        public async Task TapKeyAsync(byte keyCode, int minHoldMs = 45, int maxHoldMs = 85)
        {
            SendKeyDown(keyCode);
            int holdTime = _random.Next(minHoldMs, maxHoldMs);
            await Task.Delay(holdTime);
            SendKeyUp(keyCode);
        }

        private async void SendKeybCMD_Click(object sender, EventArgs e)
        {
            // Используем textBox1 согласно Form1.Designer.cs
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Введите символ в поле ввода клавиатуры!");
                return;
            }

            SendKeybCMD.Enabled = false;

            char c = textBox1.Text[0];
            byte keyCode = (byte)c;

            await TapKeyAsync(keyCode);

            SendKeybCMD.Enabled = true;
        }

        // =========================================================
        // 3. УПРАВЛЕНИЕ МЫШЬЮ
        // =========================================================

        public void MoveMouseRelative(int dx, int dy, int wheel = 0)
        {
            sbyte clampedDx = (sbyte)Clamp(dx, -127, 127);
            sbyte clampedDy = (sbyte)Clamp(dy, -127, 127);
            sbyte clampedWheel = (sbyte)Clamp(wheel, -127, 127);

            SendRawCommand(0x01, (byte)clampedDx, (byte)clampedDy, (byte)clampedWheel);
        }

        public async Task MoveMouseToAsync(int targetX, int targetY, int speedFactor)
        {
            speedFactor = Clamp(speedFactor, 1, 100);

            while (true)
            {
                Point currentPos = Cursor.Position;

                int deltaX = targetX - currentPos.X;
                int deltaY = targetY - currentPos.Y;

                if (Math.Abs(deltaX) <= 1 && Math.Abs(deltaY) <= 1)
                    break;

                int maxStep = Math.Max(2, (int)(speedFactor * 0.4));
                int stepX = Clamp(deltaX, -maxStep, maxStep);
                int stepY = Clamp(deltaY, -maxStep, maxStep);

                MoveMouseRelative(stepX, stepY);

                await Task.Delay(_random.Next(7, 11));
            }
        }

        private async void SendMouseCMD_Click(object sender, EventArgs e)
        {
            string[] parts = mooveCursorTextBox.Text.Split(',');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0].Trim(), out int targetX) ||
                !int.TryParse(parts[1].Trim(), out int targetY))
            {
                MessageBox.Show("Координаты должны быть в формате: X, Y (например: 500, 300)");
                return;
            }

            if (!int.TryParse(Speed.Text.Trim(), out int speedVal))
            {
                speedVal = 50;
            }

            SendMouseCMD.Enabled = false;

            await MoveMouseToAsync(targetX, targetY, speedVal);

            SendMouseCMD.Enabled = true;
        }

        // =========================================================
        // 4. СБРОС В BOOTLOADER И ЗАКРЫТИЕ
        // =========================================================

        private void button1_Click(object sender, EventArgs e)
        {
            var ask = MessageBox.Show(
                "Перевести плату в режим Bootloader на 8 секунд для перепрошивки?",
                "Сброс контроллера",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ask == DialogResult.Yes)
            {
                SendRawCommand(0xFF);

                _hidStream?.Dispose();
                _hidStream = null;
                _hidDevice = null;

                MessageBox.Show(
                    "Команда отправлена!\nПлата ушла в режим загрузчика на 8 секунд.\nМожно нажимать Upload в VS Code.",
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SendRawCommand(0x04);
            _hidStream?.Dispose();
            _hidStream = null;
            _hidDevice = null;
        }
    }
}
