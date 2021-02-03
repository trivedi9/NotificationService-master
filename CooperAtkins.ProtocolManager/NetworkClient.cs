
namespace CooperAtkins.SocketManager
{
    using System;
    using System.Text;
    using System.Net;
    using System.Net.Sockets;

    public class NetworkClient
    {
        public void TcpClient(string ipString, int port, byte[] data)
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(new IPEndPoint(IPAddress.Parse(ipString), port));

                if (client.Connected)
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                }
                client.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void UdpClient(string ipString, int port, string data)
        {
            /*Resolve the IP Address when system name is sent*/
            System.Net.IPAddress[] ipAddress = Dns.GetHostAddresses(ipString);

            UdpClient client = new UdpClient();
            try
            {

                for (int i = 0; i < ipAddress.Length; i++)
                {
                    if (ipAddress[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                       client.Connect(new IPEndPoint(IPAddress.Parse(ipAddress[i].ToString()), port));
                    }
                }
               

                byte[] buffer = Encoding.ASCII.GetBytes(data);

                client.Send(buffer, buffer.Length);
                client.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



    }
}
