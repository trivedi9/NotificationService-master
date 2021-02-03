/*
 *  File Name : NotifyTcpServer.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/20/2010
 *  
 */
namespace CooperAtkins.SocketManager
{
    using System;
    using System.Text;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using CooperAtkins.Interface.NotifyCom;

    public class NotifyTcpServer : INotificationChannelServer
    {
        
        Func<string, string, string> _receiveAction;

        bool _stopListner = false;

        // Thread signal.
        ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        string _serverName;
        int _port;
        string _endPointAddress;


        #region INotificationChannelServer Members

        public void StopListner() {
            _stopListner = true;
        }

        private void AcceptClient(IAsyncResult ar)
        {
            TcpListener server = (TcpListener)ar.AsyncState;
            TcpClient client = server.EndAcceptTcpClient(ar);

            if (!_stopListner)
                BeginAcceptTcpClient(server);



            // Buffer for reading data
            Byte[] bytes = new Byte[2048];

            Console.WriteLine("Connected!");
            string endpoint = client.Client.RemoteEndPoint.ToString();
            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            int i = 0;

            StringBuilder sb = new StringBuilder();
            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                sb.Append(System.Text.Encoding.ASCII.GetString(bytes, 0, i));
                break;
            }

            try
            {
                string response = _receiveAction(sb.ToString(), endpoint);
                byte[] bytes2Send = Encoding.ASCII.GetBytes(response);
                stream.Write(bytes2Send, 0, bytes2Send.Length);
            }
            catch (Exception ex)
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes("Remote Exception:" + ex.Message + "\n Stack Trace: " + ex.StackTrace + "\n Input: " + sb.ToString());
                stream.Write(buffer, 0, buffer.Length);
            }

            // Shutdown and end connection
            //client.Close();

            
            // Signal the calling thread to continue.
            tcpClientConnected.Set();
            
            Console.WriteLine("Completed");
        }
        
        // Accept one client connection asynchronously.
        public void BeginAcceptTcpClient(TcpListener
            listener)
        {
            // Set the event to nonsignaled state.
            tcpClientConnected.Reset();

            Console.WriteLine("Waiting for connection...");

            // Accept the connection. 
            // BeginAcceptSocket() creates the accepted socket.
            listener.BeginAcceptTcpClient(
                new AsyncCallback(AcceptClient),
                listener);

            // Wait until a connection is made and processed before 
            // continuing.
            //tcpClientConnected.WaitOne();
        }
        
        public void StartListner()
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse(_serverName);

                // TcpListener server = new TcpListener(port);
                TcpListener server = new TcpListener(localAddr, _port);

                // Start listening for client requests.
                server.Start();
                BeginAcceptTcpClient(server);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        #endregion

        #region INotificationChannel Members

        public string EndPointAddress
        {
            get
            {
                return _endPointAddress;
            }
            set
            {
                _endPointAddress = value;
                _serverName = _endPointAddress.Split(':')[0];
                _port = Convert.ToInt16(_endPointAddress.Split(':')[1]);

            }
        }

        public void OnReceive(Func<string, string, string> receiveAction)
        {
            _receiveAction = receiveAction;
        }
       

        #endregion
    }
}
