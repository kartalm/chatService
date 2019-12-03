using CrosscuttingConcernCollection.Base;
using CrosscuttingConcernCollection.Helper;
using DependencyResolution;
using System;
using System.Globalization;

namespace CrosscuttingConcernCollection.ExceptionHandling
{
    [Serializable]
    public class ApplicationException : Exception, IApplicationException
    {
        private readonly ILog _logger;

        public ApplicationException()
        {
            _logger = Injector.Resolve<ILog>(); 
        }

        public void LogException(string message)
        {
            _logger.Log(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + " Error message : " + message, LogType.Error);
        }

        public void LogException(string message, Exception inner)
        {
            _logger.Log(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + " Error message : " + message + " InnerException : " + inner.InnerException + " StackTrace : " + inner.StackTrace + " Message : " + inner.Message + " Data : " + inner.Data + " HResult : " + inner.HResult + " Source : " + inner.Source, LogType.Error);
        }

    }
}
