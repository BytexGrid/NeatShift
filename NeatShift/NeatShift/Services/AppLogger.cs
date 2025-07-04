using System;
using System.IO;

namespace NeatShift.Services
{
    /// <summary>
    /// Very lightweight file logger – writes one line per entry to
    /// %LOCALAPPDATA%\NeatShift\logs\app.log.  All I/O failures are swallowed
    /// so logging can never crash the main app.
    /// </summary>
    internal static class AppLogger
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NeatShift",
            "logs");

        private static readonly string LogFile = Path.Combine(LogDirectory, "app.log");

        public static void Log(string message)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                File.AppendAllText(LogFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");
            }
            catch
            {
                // Intentionally ignore all exceptions – logging must never break the app.
            }
        }

        public static void Log(Exception ex)
        {
            Log($"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }
} 