using System.Net;

namespace core.BusinessEntityCollection
{
    public class IpInfo
    { 
        private int _port = 15000;

        public IPHostEntry Host
        {
            get { return Dns.GetHostEntry(Dns.GetHostName()); }
        }

        public IPAddress Address
        {
            get { return IPAddress.Loopback; }
        }

        public int Port
        {
            get { return _port; }// todo get available random port or from config file
        }

        public IPEndPoint EndPoint
        {
            get { return new IPEndPoint(Address, Port); }
        }

    }
}
