using CrosscuttingConcernCollection.Base;
using CrosscuttingConcernCollection.Helper;
using NLog;
using System;

namespace CrosscuttingConcernCollection.Logging
{
    [Serializable]
    public class ApplicationLogger : Logger, ILog
    {
        private Logger _logger;
         
        public void Log(string log, LogType type)
        {
            _logger = LogManager.GetCurrentClassLogger();

            switch (type)
            {
                case LogType.Trace:
                    _logger.Trace(log);
                    break;
                case LogType.Info:
                    _logger.Info(log);
                    break;
                case LogType.Warning:
                    _logger.Warn(log);
                    break;
                case LogType.Error:
                    _logger.Error(log);
                    break;
                case LogType.Fatal:
                    _logger.Fatal(log);
                    break;
                default:
                    _logger.Trace(log);
                    break;
            }
        }

    }
}
