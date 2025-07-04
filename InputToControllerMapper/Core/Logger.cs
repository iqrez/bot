using System;
using System.Diagnostics;

namespace InputToControllerMapper
{
    /// <summary>
    /// Simple static logger that writes messages to <see cref="Trace"/> when
    /// diagnostics are enabled.
    /// </summary>
    public static class Logger
    {
        /// <summary>Enable or disable diagnostic logging.</summary>
        public static bool Enabled { get; set; }

        /// <summary>Write a diagnostic message.</summary>
        public static void Log(string message)
        {
            if (Enabled)
                Trace.WriteLine($"[{DateTime.Now:O}] {message}");
        }

        /// <summary>Write an error message with exception details.</summary>
        public static void LogError(string message, Exception ex)
        {
            if (Enabled)
                Trace.WriteLine($"ERROR [{DateTime.Now:O}] {message}: {ex}");
        }

        /// <summary>Write an error message for an exception.</summary>
        public static void LogError(Exception ex)
        {
            if (Enabled)
                Trace.WriteLine($"ERROR [{DateTime.Now:O}] {ex}");
        }
    }
}
