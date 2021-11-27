﻿using System;
using System.IO;
using System.Threading.Tasks;
using Logger.Data;
using Logger.Utilities;

namespace Logger.Core
{
    internal class Core
    {
        private static object? _saveLock;
        
        public Core() =>
            _saveLock = new object();

        public void Log(LogType logType, string message, bool logToConsole, bool logToFile, bool logToDb, LoggerContext? dbContext)
        {
            FileUtilities.CheckOldFiles();

            lock (_saveLock!)
            {
                var logDate = LoggerUtilities.GetLogDate();
                var logTime = LoggerUtilities.GetLogTime();
                
                if (logToConsole) ConsoleLog(message, logTime, logType);
                if (logToFile) FileLog(message, logTime, logType);
                if (logToDb && dbContext != null) DatabaseLog(message, $"{logDate} {logTime}", logType, dbContext);
            }
        }

        private static void ConsoleLog(string message, string time, LogType logType)
        {
            var prefix = LoggerUtilities.GetLogPrefix(logType);
            
            switch (LoggerConfiguration.LoggingStyle)
            {
                case LogStyle.Gray:
                    LoggerUtilities.ConsoleColorWrite("[", ConsoleColor.DarkGray);
                    LoggerUtilities.ConsoleColorWrite(time, ConsoleColor.Gray);
                    LoggerUtilities.ConsoleColorWrite("]", ConsoleColor.DarkGray);
                    LoggerUtilities.ConsoleColorWrite("[", ConsoleColor.DarkGray);
                    LoggerUtilities.ConsoleColorWrite(prefix, ConsoleColor.Gray);
                    LoggerUtilities.ConsoleColorWrite("]", ConsoleColor.DarkGray);
                    LoggerUtilities.ConsoleColorWrite($" {message}", Console.ForegroundColor, true);
                    break;
                
                case LogStyle.OneColor:
                    Console.WriteLine($"[{time}][{prefix}] {message}");
                    break;
                
                case LogStyle.Minimalistic:
                    LoggerUtilities.ConsoleColorWrite(time, ConsoleColor.Gray);
                    LoggerUtilities.ConsoleColorWrite($" {prefix}", ConsoleColor.Gray);
                    LoggerUtilities.ConsoleColorWrite($" {message}", Console.ForegroundColor, true);
                    break;
                
                case LogStyle.Default:
                    var prefixColor = LoggerUtilities.GetLogPrefixColor(logType);
                    
                    LoggerUtilities.ConsoleColorWrite("[", ConsoleColor.DarkGray);
                    LoggerUtilities.ConsoleColorWrite(time, ConsoleColor.Gray);
                    LoggerUtilities.ConsoleColorWrite("]", ConsoleColor.DarkGray);
                    LoggerUtilities.ConsoleColorWrite("[", ConsoleColor.DarkGray);
                    LoggerUtilities.ConsoleColorWrite(prefix, prefixColor);
                    LoggerUtilities.ConsoleColorWrite("]", ConsoleColor.DarkGray);
                    LoggerUtilities.ConsoleColorWrite($" {message}", Console.ForegroundColor, true);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void FileLog(string message, string time, LogType logType)
        {
            var fileName = FileUtilities.GetLogFileName();
            var logPath = FileUtilities.GetPathToLogFile(fileName);
            var prefix = LoggerUtilities.GetLogPrefix(logType);
            
            var sw = new StreamWriter(logPath, true);
            
            switch (LoggerConfiguration.LoggingStyle)
            {
                case LogStyle.Minimalistic:
                    sw.WriteLine($"{time} {prefix} {message}");
                    break;
                
                default:
                    sw.WriteLine($"[{time}][{prefix}] {message}");
                    break;
            }
            
            sw.Close();
        }

        private static void DatabaseLog(string message, string time, LogType logType, LoggerContext context)
        {
            var log = new Log()
            {
                LogType = logType,
                DateTime = DateTime.Parse(time),
                Message = message
            };

            context.Logs.Add(log);
            context.SaveChanges();
        }
    }
}