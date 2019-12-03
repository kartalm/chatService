using core.BusinessEntityCollection;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace core.Base
{
    public abstract class Client
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public StringBuilder Data { get; set; }

        public bool Status { get; set; }

        public TcpClient TcpClient { get; set; }

        public EventWaitHandle Handle { get; set; }

        public NetworkStream Stream { get; set; }

        public ClientBuffer Buffer { get; set; }

    }
}
