using CrosscuttingConcernCollection.Helper;

namespace CrosscuttingConcernCollection.Base
{
    public interface ILog
    {
        void Log(string log, LogType type);

    }
}
