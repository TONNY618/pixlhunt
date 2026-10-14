using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelMacroEngine.Core.Services
{
    /// <summary>
    /// Сервис воспроизведения звуков через MCI (winmm.dll).
    /// Не блокирует вызывающий поток: команда play возвращается сразу.
    /// </summary>
    public static class SoundService
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
        private static extern int mciSendString(string lpstrCommand, StringBuilder? lpstrReturnString, int uReturnLength, IntPtr hwndCallback);

        /// <summary>
        /// Воспроизводит аудиофайл из папки Sounds/Favorite.
        /// </summary>
        public static void Play(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;

            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Favorite", fileName);
            if (!File.Exists(basePath))
            {
                string fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Sounds", "Favorite", fileName);
                if (File.Exists(fallback)) basePath = Path.GetFullPath(fallback);
                else return;
            }

            string alias = "snd_" + Guid.NewGuid().ToString("N");
            string ext = Path.GetExtension(basePath).ToLowerInvariant();

            string openCmd = ext == ".mp3"
                ? $"open \"{basePath}\" type mpegvideo alias {alias}"
                : $"open \"{basePath}\" alias {alias}";

            mciSendString(openCmd, null, 0, IntPtr.Zero);
            mciSendString($"play {alias}", null, 0, IntPtr.Zero);
        }
    }
}
