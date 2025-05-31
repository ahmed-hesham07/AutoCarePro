using System;
using System.IO;
using System.Threading.Tasks;

namespace AutoCarePro.Services
{
    /// <summary>
    /// Provides centralized logging functionality for the application.
    /// Logs are stored in daily files within the Logs directory.
    /// </summary>
    public class LoggingService
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string LogFile = Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
        private static readonly object LockObject = new object();

        /// <summary>
        /// Initializes the logging service by creating the Logs directory if it doesn't exist.
        /// </summary>
        public static void Initialize()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        /// <summary>
        /// Logs an error message with optional exception details.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="ex">Optional exception to include in the log entry.</param>
        public static void LogError(string message, Exception? ex = null)
        {
            Log("ERROR", message, ex);
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The information message to log.</param>
        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        public static void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        /// <summary>
        /// Internal method to write log entries to the log file.
        /// </summary>
        /// <param name="level">The log level (ERROR, INFO, WARNING).</param>
        /// <param name="message">The message to log.</param>
        /// <param name="ex">Optional exception to include in the log entry.</param>
        private static void Log(string level, string message, Exception? ex = null)
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                if (ex != null)
                {
                    logMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
                    if (ex.InnerException != null)
                    {
                        logMessage += $"\nInner Exception: {ex.InnerException.Message}";
                    }
                }

                lock (LockObject)
                {
                    File.AppendAllText(LogFile, logMessage + Environment.NewLine);
                }
            }
            catch
            {
                // If logging fails, we don't want to throw another exception
                // This is a last resort fallback
            }
        }
    }
} 