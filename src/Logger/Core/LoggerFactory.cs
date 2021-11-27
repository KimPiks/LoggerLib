﻿using System;
using Logger.Data;
using Logger.Utilities;
using Microsoft.Extensions.Logging;

namespace Logger.Core
{
    internal class LoggerFactory : ILogger
    {
        private readonly string _name;

        public LoggerFactory(
            string name) =>
            _name = name;

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel)
        {
            var logType = LoggerUtilities.ConvertLogType(logLevel);
            if (LoggerConfiguration.LogAllLogLevels)
                return true;

            if (LoggerConfiguration.RequiredLogLevel != null)
                return logType == LoggerConfiguration.RequiredLogLevel;

            return (int) LoggerConfiguration.MinimumLogLevel <= (int) logType;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var logType = LoggerUtilities.ConvertLogType(logLevel);
            if (!IsEnabled(logLevel)) return;

            Logger.Log(logType, $"|{_name}| - {formatter(state, exception)}");
        }
    }
}