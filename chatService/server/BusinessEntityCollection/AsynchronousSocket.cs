using core.Base;
using core.BusinessEntityCollection;
using CrosscuttingConcernCollection.Base;
using CrosscuttingConcernCollection.Helper;
using DependencyResolution;
using server.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace server.EntityCollection
{
    public class AsynchronousSocket : IAsyncSocket
    {
        #region Declarations

        private readonly ILog _logger;
        private readonly IMonitorable _monitor;
        private readonly IApplicationException _applicationException;

        private ConcurrentDictionary<string, ChatClient> _chatClientCollection;
        private IpInfo _ipInfo;
        private Task _send = null;
        private bool _isActive = false;
        private bool _isExit = false;

        #endregion

        public AsynchronousSocket()
        {
            #region Dependencies

            _logger = Injector.Resolve<ILog>();
            _applicationException = Injector.Resolve<IApplicationException>();
            _monitor = Injector.Resolve<IMonitorable>();

            #endregion 

            _chatClientCollection = new ConcurrentDictionary<string, ChatClient>();
        }
         
        public void Connect(Client client)
        { 
            var _client = (ChatClient)client; 
            _chatClientCollection.TryAdd(_client.Id, _client);
            _monitor.Display("Client " + _client.Id + " connected");
            _logger.Log("Client with Id : " + client.Id.ToString() + " connected", LogType.Info);
            if (_client.Stream.CanRead && _client.Stream.CanWrite)
            {
                while (_client.TcpClient.Connected)
                {
                    try
                    {
                        _client.Stream.BeginRead(_client.Buffer.Data, 0, _client.Buffer.Size, new AsyncCallback(Read), _client);
                        _client.Handle.WaitOne();
                    }
                    catch (IOException ex) 
                    { 
                        _monitor.Display("Error occured. Details : " + ex);
                        _applicationException.LogException("Error occured. Details : " + ex);
                    }
                    catch (ObjectDisposedException ex) 
                    { 
                        _monitor.Display("Error occured. Details : " + ex);
                        _applicationException.LogException("Error occured. Details : " + ex); 
                    }
                }
            }
            else
            {
                _monitor.Display("Error occured. Unable to read and write messages.");
                _logger.Log("Error occured. Unable to read and write messages.", LogType.Error);
            }
            _client.TcpClient.Close();
            _chatClientCollection.TryRemove(_client.Id, out ChatClient tmp);
            _monitor.Display("Client " + _client.Id + " disconnected");
            _logger.Log("Client " + _client.Id + " disconnected", LogType.Warning);
        }

        public void DisconnectAll()
        {
            foreach (KeyValuePair<string, ChatClient> client in _chatClientCollection)
            {
                client.Value.TcpClient.Close();
            }
            _monitor.Display("All clients are disconnected");
            _logger.Log("All clients are disconnected", LogType.Info);
        }

        public void Disconnect(Client client)
        {
            var _client = (ChatClient)client;
            foreach (KeyValuePair<string, ChatClient> clt in _chatClientCollection)
            {
                if (_client.Id == clt.Value.Id) 
                {
                    _client.TcpClient.Close();
                    _monitor.Display("Client with Id : " + _client.Id + "disconnected");
                    _logger.Log("Client with Id : " + _client.Id + "disconnected", LogType.Warning);
                    break;
                } 
            }
        }

        public void Listen()
        { 
            _ipInfo = new IpInfo();
            _monitor.Display("Server IP Address : " + _ipInfo.Address + " & Port : " + _ipInfo.Port.ToString());
            _logger.Log("Server IP Address : " + _ipInfo.Address + " & Port : " + _ipInfo.Port.ToString(), LogType.Info);
            var listener = new TcpListener(_ipInfo.Address, _ipInfo.Port);
            try
            { 
                listener.Start();
                SetStatus(true);
                while (_isActive)
                {
                    if (listener.Pending())
                    {
                        var client = new ChatClient(new StringBuilder(), listener.AcceptTcpClient(), new EventWaitHandle(false, EventResetMode.AutoReset), new ClientBuffer());
                        client.Name = "client" + client.Id;
                        client.Stream = client.TcpClient.GetStream();
                        var connectionThread = new Thread(() => Connect(client))
                        {
                            IsBackground = true
                        };
                        connectionThread.Start(); 
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                listener.Server.Close();
                SetStatus(false);
            }
            catch (SocketException ex) 
            { 
                _monitor.Display("Error occured. Details : " + ex);
                _applicationException.LogException("Error occured. Details : " + ex);
            }
        }

        public void Read(IAsyncResult result)
        {
            var client = (ChatClient)result.AsyncState;
            int bytes = 0;
            if (client.IsNotNull() && client.TcpClient.Connected)
            {
                try
                {
                    bytes = client.Stream.EndRead(result);
                }
                catch (IOException ex) 
                { 
                    _monitor.Display("Error occured. Details : " + ex);
                    _applicationException.LogException("Error occured. Details : " + ex);
                }
                catch (ObjectDisposedException ex) 
                { 
                    _monitor.Display("Error occured. Details : " + ex);
                    _applicationException.LogException("Error occured. Details : " + ex);
                }
            }
            if (bytes > 0)
            {
                client.Data.AppendFormat("{0}", Encoding.UTF8.GetString(client.Buffer.Data, 0, bytes));
                var isDataAvailable = false;
                try
                {
                    isDataAvailable = client.Stream.DataAvailable;
                }
                catch (IOException ex) 
                { 
                    _monitor.Display("Error occured. Details : " + ex);
                    _applicationException.LogException("Error occured. Details : " + ex);
                }
                catch (ObjectDisposedException ex) 
                { 
                    _monitor.Display("Error occured. Details : " + ex);
                    _applicationException.LogException("Error occured. Details : " + ex);
                }
                if (isDataAvailable)
                {
                    try
                    {
                        client.Stream.BeginRead(client.Buffer.Data, 0, client.Buffer.Size, new AsyncCallback(Read), client);
                    }
                    catch (IOException ex)
                    {
                        _monitor.Display("Error occured. Details : " + ex);
                        _applicationException.LogException("Error occured. Details : " + ex);
                        client.Handle.Set();
                    }
                    catch (ObjectDisposedException ex)
                    {
                        _monitor.Display("Error occured. Details : " + ex);
                        _applicationException.LogException("Error occured. Details : " + ex);
                        client.Handle.Set();
                    }
                }
                else
                {
                    var message = string.Format(client.Data.ToString());
                    _logger.Log("Client with Id : " + client.Id.ToString() + " message : " + message, LogType.Info);
                    _monitor.Display(message);
                    if (_send.IsNull() || _send.IsCompleted)
                    {
                        _send = Task.Factory.StartNew(() => Send(message, client.Id));
                    }
                    else
                    {
                        _send.ContinueWith(antecendent => Send(message, client.Id));
                    }
                    client.Data.Clear();
                    client.Handle.Set();
                }
            }
            else
            {
                client.TcpClient.Close();
                client.Handle.Set();
                _logger.Log("Client with Id : " + client.Id.ToString() + " disconnected", LogType.Warning);
            }
        }

        public void Send(string message, string id)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            foreach (KeyValuePair<string, ChatClient> client in _chatClientCollection)
            {
                if (id != client.Value.Id)
                {
                    try
                    {
                        client.Value.Stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), client.Value);
                    }
                    catch (IOException ex) 
                    { 
                        _monitor.Display("Error occured. Details : " + ex);
                        _applicationException.LogException("Error occured. Details : " + ex);
                    }
                    catch (ObjectDisposedException ex) 
                    { 
                        _monitor.Display("Error occured. Details : " + ex);
                        _applicationException.LogException("Error occured. Details : " + ex);
                    }
                }
            }
        }

        public void SetStatus(bool status)
        {
            if (!_isExit)
            {
                _isActive = status;
                if (status)
                {
                    _monitor.Display("Server started at " + DateTime.Now);
                    _logger.Log("Server started at " + DateTime.Now, LogType.Info);
                }
                else
                {
                    _monitor.Display("Server stopped at " + DateTime.Now);
                    _logger.Log("Server stopped at " + DateTime.Now, LogType.Info);
                }
            }
        }

        public void Write(IAsyncResult result)
        {
            var client = (ChatClient)result.AsyncState;
            if (client.TcpClient.Connected)
            {
                try
                {
                    client.Stream.EndWrite(result);
                }
                catch (IOException ex) 
                { 
                    _monitor.Display("Error occured. Details : " + ex);
                    _applicationException.LogException("Error occured. Details : " + ex);
                }
                catch (ObjectDisposedException ex) 
                { 
                    _monitor.Display("Error occured. Details : " + ex);
                    _applicationException.LogException("Error occured. Details : " + ex);
                }
            }
        }

    }

}
