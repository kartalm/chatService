namespace core.BusinessEntityCollection
{
    public class ClientBuffer
    {
        private static int _size = 1024;//todo read size from config file 
        private byte[] _buffer = new byte[_size];

        public int Size
        {
            get { return _size; }
        }

        public byte[] Data
        {
            get { return _buffer;  }
            set { _buffer = value; }
        }

    }
}
