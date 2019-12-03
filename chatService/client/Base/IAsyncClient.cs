using core.BusinessEntityCollection;
using System;

namespace client.Base
{
    public interface IAsyncClient
    {
        void Connect();
          
        void Read(IAsyncResult result);

        void Write(IAsyncResult result);

        void Send(string message);

        void SetStatus(bool status);

        void Disconnect();

    }
}
