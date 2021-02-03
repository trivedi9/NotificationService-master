
/*
 *  File Name : NetworkListner.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  
 */

namespace CooperAtkins.SocketManager
{

    using System.Net.Sockets;
    using System.Net;
    using System.Threading;


    public class NetworkListner
    {
        NetworkStream _stream;

        public static NetworkListner Create()
        {
            return new NetworkListner();
        }

        private NetworkListner()
        {

        }
        public void Send(string obj)
        {
        }
        public string Receive()
        {
            return "";
        }
        private void TcpListner(int port)
        {
            TcpListener listner = new TcpListener(System.Net.IPAddress.Any, port);
            listner.Start();

            Thread thread4Listner = new Thread(() =>
            {

                byte[] bytes = new byte[1024];
                int index = 0;
                string data = "";
                while (true)
                {
                    TcpClient tcpClient = listner.AcceptTcpClient();

                    _stream = tcpClient.GetStream();

                    while ((index = _stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, index);

                    }
                    tcpClient.Close();

                }
            });

            thread4Listner.IsBackground = true;
            thread4Listner.Start();

        }

       
    }
}
