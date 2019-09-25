using System;
using System.IO;
using NLog;

namespace Helpers
{
    public class LoggerHelper
    {
        Logger Logger;
        string LogPath;
        private LoggerHelper() { }

        public static LoggerHelper CreateLoggerHelper()
        {
            var logger = new LoggerHelper();
            logger.Logger = LogManager.GetCurrentClassLogger();
            logger.LogPath = System.IO.Path.Combine(DateTime.Now.ToString("yyyy-MM-dd"));
            return logger;
        }

        public static LoggerHelper CreateLoggerHelper(string path)
        {
            var logger = new LoggerHelper();
            logger.Logger = LogManager.GetCurrentClassLogger();
            logger.LogPath = path;
            return logger;
        }

        public void Log(LogLevel level, string message)
        {
            LogEventInfo logEventInfo = new LogEventInfo()
            {
                Level = level,
                Message = message,
            };
            logEventInfo.Properties["path"] = Path.Combine(LogPath, $"{level.Name}.log");
            Logger.Log(logEventInfo);
        }
    }

}
