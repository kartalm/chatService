using System.Net.Sockets;
using System.Text;

namespace core.BusinessEntityCollection
{
    public class ClientState
    { 
        public Socket Socket { get; set; }

        public ClientBuffer ClientBuffer { get; set; }

        public StringBuilder Data { get; set; }

    }
}
