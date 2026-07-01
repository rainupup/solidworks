using System;
using System.IO;

namespace SolidWorks.ParametricAddin.Helpers
{
    /// <summary>
    /// Simple file-based logger for debugging crashes.
    /// </summary>
    public static class Logger
    {
        private static readonly string LogPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "sw_addin_log.txt");

        private static readonly object _lock = new object();

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message, Exception ex = null)
        {
            var text = ex != null ? $"{message} | Exception: {ex}" : message;
            Write("ERROR", text);
        }

        public static void Trace(string message)
        {
            Write("TRACE", message);
        }

        private static void Write(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(LogPath,
                        $"{DateTime.Now:HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}");
                }
            }
            catch { }
        }

        public static string GetLogPath() => LogPath;
    }
}
