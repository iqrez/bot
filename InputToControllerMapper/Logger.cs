using System;
using System.IO;

namespace InputToControllerMapper
{
    public static class Logger
    {
        public enum LogLevel { Info, Warning, Error }

        private static readonly object lockObj = new();
        private static StreamWriter? writer;

        public static event Action<LogLevel, string>? LogMessage;

        public static void Initialize(string filePath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                writer = new StreamWriter(filePath, append: true) { AutoFlush = true };
                LogInfo("Logger initialised");
            }
            catch
            {
                // If the log file can't be created we continue without file logging
                writer = null;
            }
        }

        public static void Shutdown()
        {
            lock (lockObj)
            {
                writer?.Dispose();
                writer = null;
            }
        }

        public static void LogInfo(string message) => Log(LogLevel.Info, message);
        public static void LogWarning(string message) => Log(LogLevel.Warning, message);
        public static void LogError(string message) => Log(LogLevel.Error, message);

        private static void Log(LogLevel level, string message)
        {
            string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            lock (lockObj)
            {
                writer?.WriteLine(entry);
            }
            LogMessage?.Invoke(level, entry);
        }
    }
}
