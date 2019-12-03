using core.Base;
using System;

namespace server.Base
{
    public interface IAsyncSocket
    {
        void Connect(Client obj);

        void Listen();

        void Read(IAsyncResult result);

        void Write(IAsyncResult result);

        void Send(string message, string id);

        void Disconnect(Client client);

        void DisconnectAll();

        void SetStatus(bool status);
         
    }
}
