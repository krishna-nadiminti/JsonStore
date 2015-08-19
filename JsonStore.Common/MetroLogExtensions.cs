using System;
using MetroLog;

namespace Money.Extensions
{
    public static class MetroLogExtensions
    {
        private static bool _isInDebugMode; //one global var to control logging for all loggers

        /// <summary>
        /// Extension to get a logger based on the type of the caller
        /// </summary>
        /// <param name="anyObject"></param>
        /// <param name="isInDesignMode"></param>
        /// <returns></returns>
        public static ILogger GetLogger(this object anyObject, bool isInDesignMode = false)
        {
            if (isInDesignMode)
            {
                return new NullLogger();
            }

            if (anyObject == null) 
                return LogManagerFactory.DefaultLogManager.GetLogger<object>();

            //useful for static classes, where we can say something like: typeof(LifeCycleManager).GetLogger()
            var objectType = anyObject as Type;
            if (objectType != null)
                return LogManagerFactory.DefaultLogManager.GetLogger(objectType);

            return LogManagerFactory.DefaultLogManager.GetLogger(anyObject.GetType());
        }

        public static void DebugEx(this ILogger logger, string message, params object[] args)
        {
#if !DEBUG
            if (_isInDebugMode)
#endif
                logger.Debug(message, args);
        }

        public static void LogTime(this ILogger logger, string message, params object[] args)
        {
            DebugEx(logger, DateTime.Now.ToString("hh:mm:ss.fff ") + message, args);
        }

        public static void SetDebugMode(bool isInDebugMode)
        {
            //not using locking, because it doesn't matter if one/two debug messages are in a race condition and get lost.
            //perf is more important
            _isInDebugMode = isInDebugMode;
        }
    }

    public class NullLogger : ILogger
    {
        public void Trace(string message, Exception ex = null)
        {            
        }

        public void Trace(string message, params object[] ps)
        {
        }

        public void Debug(string message, Exception ex = null)
        {
        }

        public void Debug(string message, params object[] ps)
        {
        }

        public void Info(string message, Exception ex = null)
        {
        }

        public void Info(string message, params object[] ps)
        {
        }

        public void Warn(string message, Exception ex = null)
        {
        }

        public void Warn(string message, params object[] ps)
        {
        }

        public void Error(string message, Exception ex = null)
        {
        }

        public void Error(string message, params object[] ps)
        {
        }

        public void Fatal(string message, Exception ex = null)
        {
        }

        public void Fatal(string message, params object[] ps)
        {
        }

        public void Log(LogLevel logLevel, string message, Exception ex)
        {
        }

        public void Log(LogLevel logLevel, string message, params object[] ps)
        {
        }

        public bool IsEnabled(LogLevel level)
        {
            return false;
        }

        public string Name
        {
            get { return "NullLogger"; }
        }

        public bool IsTraceEnabled
        {
            get; 
            set;
        }

        public bool IsDebugEnabled
        {
            get;
            set;
        }

        public bool IsInfoEnabled
        {
            get;
            set;
        }

        public bool IsWarnEnabled
        {
            get; 
            set;
        }

        public bool IsErrorEnabled
        {
            get;
            set;
        }

        public bool IsFatalEnabled
        {
            get;
            set;
        }
    }
}
