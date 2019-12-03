using System;

namespace CrosscuttingConcernCollection.Base
{
    public interface IApplicationException
    {
        void LogException(string message);

        void LogException(string message, Exception inner);

    }
}
