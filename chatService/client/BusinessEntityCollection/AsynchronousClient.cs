using client.Base;
using core.Base;
using core.BusinessEntityCollection;
using CrosscuttingConcernCollection.Base;
using CrosscuttingConcernCollection.Helper;
using CrosscuttingConcernCollection.Monitoring;
using DependencyResolution;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace client.BusinessEntityCollection
{
    public class AsynchronousClient : IAsyncClient
    {
        #region Declarations

        private readonly ILog _logger;
        private readonly IMonitorable _monitor;
        private readonly IApplicationException _applicationException;

        private ChatClient _chatClient;  
        private Task _send = null;
        private bool _isExit = false;
        private bool _connected = false;

        #endregion

        public AsynchronousClient()
        {
            #region Dependencies

            _logger = Injector.Resolve<ILog>();
            _applicationException = Injector.Resolve<IApplicationException>();
            _monitor = Injector.Resolve<IMonitorable>();

            #endregion 
        }

        public void SetStatus(bool status)
        {
            if (!_isExit)
            {
                _connected = status;
                if (status)
                {
                    _monitor.Display("Connected to server at " + DateTime.Now);
                    _logger.Log("Connected to server at " + DateTime.Now, LogType.Info);
                }
                else
                {
                    _monitor.Display("Disconnected from server at " + DateTime.Now);
                    _logger.Log("Disconnected from server at " + DateTime.Now, LogType.Info);
                }
            }
        }
         
        public void Connect()
        {
            var ipInfo = new IpInfo();
            var tcpClient = new TcpClient();
            _chatClient = new ChatClient(new StringBuilder(), tcpClient, new EventWaitHandle(false, EventResetMode.AutoReset), new ClientBuffer());
            _chatClient.Name = "client" + _chatClient.Id;
            _monitor.Display("Server IP Address : " + ipInfo.Address + " & Port : " + ipInfo.Port.ToString());
            _monitor.Display(_chatClient.Name);
            try
            {
                _chatClient.TcpClient.Connect(ipInfo.Address, ipInfo.Port);
                _chatClient.Stream = _chatClient.TcpClient.GetStream();
                SetStatus(true);
                StartMessaging(_chatClient.Id);
                if (_chatClient.Stream.CanRead && _chatClient.Stream.CanWrite)
                {
                    while (_chatClient.TcpClient.Connected)
                    {
                        try
                        {
                            _chatClient.Stream.BeginRead(_chatClient.Buffer.Data, 0, _chatClient.Buffer.Size, new AsyncCallback(Read), null);
                            _chatClient.Handle.WaitOne();
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
                    _monitor.Display("Unable to read data");
                    _logger.Log("Unable to read data", LogType.Error);
                }
                _chatClient.TcpClient.Close();
                SetStatus(false);
            }
            catch (Exception ex)
            {
                _monitor.Display("Error occured. Details : " + ex);
                _applicationException.LogException("Error occured. Details : " + ex);
            }
        }

        public void Read(IAsyncResult result)
        {
            var chatClient = (ChatClient)result.AsyncState;
            int bytes = 0;
            if (chatClient.IsNotNull() && chatClient.TcpClient.Connected)
            {
                try
                {
                    bytes = chatClient.Stream.EndRead(result);
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
                chatClient.Data.AppendFormat("{0}", Encoding.UTF8.GetString(chatClient.Buffer.Data, 0, bytes));
                var isDataAvailable = false;
                try
                {
                    isDataAvailable = chatClient.Stream.DataAvailable;
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
                        chatClient.Stream.BeginRead(chatClient.Buffer.Data, 0, chatClient.Buffer.Size, new AsyncCallback(Read), null);
                    }
                    catch (IOException ex)
                    {
                        _monitor.Display("Error occured. Details : " + ex);
                        _applicationException.LogException("Error occured. Details : " + ex);
                        chatClient.Handle.Set();
                    }
                    catch (ObjectDisposedException ex)
                    {
                        _monitor.Display("Error occured. Details : " + ex);
                        _applicationException.LogException("Error occured. Details : " + ex);
                        chatClient.Handle.Set();
                    }
                }
                else
                {
                    _monitor.Display(chatClient.Data.ToString());
                    chatClient.Data.Clear();
                    chatClient.Handle.Set();
                }
            }
            else
            {
                chatClient.TcpClient.Close();
                chatClient.Handle.Set();
                _logger.Log("Client disconnected at " + DateTime.Now, LogType.Warning);
            }
        }

        public void Send(string message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                _chatClient.Stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), null);
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

        public void StartMessaging(string message)
        {
            if(message.Length > 0 && _connected)
            {
                if (_send.IsNull() || _send.IsCompleted)
                {
                    _send = Task.Factory.StartNew(() => Send(message));
                }
                else
                {
                    _send.ContinueWith(antecedent => Send(message));
                }
            }
            else
            {
                _monitor.Display("Unable to start messaging");
                _logger.Log("Unable to start messaging", LogType.Error);
            }
        }

        public void Write(IAsyncResult result)
        {
            var chatClient = (ChatClient)result.AsyncState;
            if (chatClient.IsNotNull() && chatClient.TcpClient.Connected)
            {
                try
                {
                    chatClient.Stream.EndWrite(result);
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

        public void Disconnect()
        {
            if (_connected)
            {
                _isExit = true;
                _chatClient.TcpClient.Close();
                _monitor.Display("Client disconnected");
                _logger.Log("Client disconnected", LogType.Warning);
            }
        }

    }
}
