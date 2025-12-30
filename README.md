SimpleLogger
============

A thread-safe, asynchronous, and easy-to-use logging utility for C# applications.
Supports log levels, console and file output, and automatic file rotation.

Created by kira024: https://github.com/kira024
Get Latest Version of SimpleLogger: https://github.com/kiraa024/SimpleLogger/blob/main/files/SimpleLogger.cs

Features
--------

- Thread-safe and asynchronous logging
- Console output with colored messages
- Log levels: Info, Warning, Error, Debug
- File output with automatic rotation when max size is exceeded
- Easy-to-use convenience methods: Info(), Warn(), Error(), Debug()

Installation
------------

Add `SimpleLogger.cs` to your project and import the namespace:

    using Utilities;

Usage Example
-------------

    using System;
    using System.Threading.Tasks;
    using Utilities;

    class Program
    {
        static async Task Main()
        {
            using var logger = new SimpleLogger(logDirectory: "logs", logFilePrefix: "myApp", maxFileSizeMB: 2);
            logger.MinimumLevel = LogLevel.Debug;

            logger.Info("Application started");
            logger.Debug("Debug info here");
            logger.Warn("This is a warning!");
            logger.Error("An error occurred");

            for (int i = 0; i < 10; i++)
            {
                logger.Info($"Processing item {i}");
                await Task.Delay(100);
            }

            Console.WriteLine("Logging complete!");
        }
    }

Configuration Options
---------------------

- logDirectory – Folder to store log files (default: logs)
- logFilePrefix – Prefix for log files (default: app)
- maxFileSizeMB – Maximum log file size before rotation (default: 5 MB)
- MinimumLevel – Minimum log level to record (default: Info)

License
-------

MIT License – free to use, modify, and distribute.
