// SimpleLogger.cs
// Created by kiraa024
// GitHub: https://github.com/kiraa024

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Debug
    }

    public class SimpleLogger : IDisposable
    {
        private readonly string logDirectory;
        private readonly string logFilePrefix;
        private readonly int maxFileSizeMB;
        private readonly BlockingCollection<string> logQueue = new();
        private readonly CancellationTokenSource cts = new();
        private Task? logTask;
        private readonly object fileLock = new();

        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

        public SimpleLogger(string logDirectory = "logs", string logFilePrefix = "app", int maxFileSizeMB = 5)
        {
            this.logDirectory = logDirectory;
            this.logFilePrefix = logFilePrefix;
            this.maxFileSizeMB = maxFileSizeMB;

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            StartLoggingTask();
        }

        private void StartLoggingTask()
        {
            logTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (logQueue.TryTake(out string logEntry, Timeout.Infinite, cts.Token))
                    {
                        WriteToConsole(logEntry);
                        await WriteToFileAsync(logEntry);
                    }
                }
            }, cts.Token);
        }

        private void WriteToConsole(string message)
        {
            if (message.Contains("[Error]")) Console.ForegroundColor = ConsoleColor.Red;
            else if (message.Contains("[Warning]")) Console.ForegroundColor = ConsoleColor.Yellow;
            else if (message.Contains("[Debug]")) Console.ForegroundColor = ConsoleColor.Cyan;
            else Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(message);
            Console.ResetColor();
        }

        private async Task WriteToFileAsync(string message)
        {
            string filePath = GetCurrentLogFilePath();

            lock (fileLock)
            {
                File.AppendAllText(filePath, message + Environment.NewLine, Encoding.UTF8);
            }
        }

        private string GetCurrentLogFilePath()
        {
            string filePath = Path.Combine(logDirectory, $"{logFilePrefix}.log");
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > maxFileSizeMB * 1024 * 1024)
                {
                    string archiveName = Path.Combine(logDirectory,
                        $"{logFilePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    File.Move(filePath, archiveName);
                }
            }
            return filePath;
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (level < MinimumLevel) return;

            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            logQueue.Add(logEntry);
        }

        // Convenience methods
        public void Info(string message) => Log(message, LogLevel.Info);
        public void Warn(string message) => Log(message, LogLevel.Warning);
        public void Error(string message) => Log(message, LogLevel.Error);
        public void Debug(string message) => Log(message, LogLevel.Debug);

        public void Dispose()
        {
            cts.Cancel();
            logQueue.CompleteAdding();
            logTask?.Wait();
            cts.Dispose();
        }
    }
}
