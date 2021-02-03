/*
 *  File Name : NotifyTcpClient.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/20/2010
 *  
 */
namespace CooperAtkins.SocketManager
{
    using System;
    using System.Net.Sockets;
    using System.ComponentModel.Composition;
    using System.Text;
    using CooperAtkins.Interface.NotifyCom;

    [Export(typeof(INotificationChannelClient))]
    public class NotifyTcpClient : INotificationChannelClient
    {
        string _server;
        int _port;
        string _endPointAddress;
        Action<string, string> _receiveAction;
        public NotifyTcpClient()
        {
        }

        #region INotificationChannelClient Members

        public bool Connect()
        {
            throw new NotImplementedException();
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
                _server = _endPointAddress.Split(':')[0];
                _port = Convert.ToInt16(_endPointAddress.Split(':')[1]);

            }
        }

        public void OnReceive(Action<string, string> receiveAction)
        {
            _receiveAction = receiveAction;
        }

        public void Send(string data)
        {
            // String to store the response ASCII representation.
            String responseData = String.Empty;


            TcpClient client = new TcpClient(_server, _port);

            // Translate the passed message into ASCII and store it as a Byte array.
            Byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);

            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();

            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer. 
            stream.Write(buffer, 0, buffer.Length);

            // Receive the TcpServer.response.
            // Buffer to store the response bytes.
            buffer = new Byte[2048];

            StringBuilder sb = new StringBuilder();

            try
            {

                int i = 0;
                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    sb.Append(System.Text.Encoding.ASCII.GetString(buffer, 0, i));
                    break;
                }
                responseData = sb.ToString();
            }
            catch (Exception ex)
            {
                responseData = "Error has occurred while read the data from network stream" + ex.Message;
            }

            // Close everything.
            stream.Close();
            client.Close();

            _receiveAction(responseData, EndPointAddress);
        }

        #endregion
    }
}
