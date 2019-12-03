using core.Base;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace core.BusinessEntityCollection
{
    public class ChatClient : Client
    { 
        public ChatClient(StringBuilder data, TcpClient tcpClient, EventWaitHandle handle, ClientBuffer buffer)
        {
            Id = Guid.NewGuid().ToString();
            Data = data;
            TcpClient = tcpClient;
            Handle = handle; 
            Buffer = buffer;
        } 
    }
}
