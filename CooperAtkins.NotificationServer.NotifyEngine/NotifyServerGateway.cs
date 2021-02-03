/*
 *  File Name : NotifyServerGateway.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/19/2010
 *  
 */
namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Threading;
    using System.Reflection;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.NotifyCom;

    /// <summary>
    /// Notification Listener Interface
    /// </summary>
    public class NotifyServerGateway
    {
        INotificationChannelServer _server = null;
        NotificationReceiver _receiver = null;


        public bool EnableLog = true;

        public NotifyServerGateway()
        {
            _receiver = new NotificationReceiver();
        }
        /// <summary>
        /// Establish communication channel object
        /// </summary>
        public void Open()
        {
            //Import configured end point class
            string[] typeParts = NotifyConfiguration.Instance.Configuration.Type.Split(',');
            Assembly assembly = Assembly.Load(typeParts[1].Trim());
            // Get the type to use.
            Type protocolWrapper = assembly.GetType(typeParts[0]);
            // Create an instance.
            _server = (INotificationChannelServer)Activator.CreateInstance(protocolWrapper);
            _server.EndPointAddress = NotifyConfiguration.Instance.Configuration.EndpointAddress;

            _server.OnReceive((data, remEndpoint) =>
            {
                if (EnableLog)
                    LogBook.Write("\r\nRemote IP:" + remEndpoint + " Data:" + data);
                string retrunText = string.Empty;
                if (data.StartsWith("command "))
                {
                    retrunText = RunCommand(data);
                }
                else
                {
                    INotifyObject notifyObject = _receiver.PrepareNotifyObject(data);
                    NotifyComResponse response = _receiver.Execute(notifyObject);

                    if (response != null)
                        retrunText = response.GetXML();
                    else
                        retrunText = "[NULL : NotifyComResponse object was not created by target component.]";
                }
                return retrunText;
            });
        }
        private string RunCommand(string command)
        {
            string[] commands = command.Split(' ');
            string output = string.Empty;
            switch (commands[1].ToLower())
            {
                case "ping":
                    output = "Notification Server: Packet received.";
                    break;
                default:
                    output = "Notification Server: Invalid command.";
                    break;

            }
            return output;
        }
        public void Start()
        {
            Thread listner = new Thread(() =>
            {
                _server.StartListner();
            });
            listner.IsBackground = true;
            listner.Start();

        }
        public void Stop()
        {
            try
            {
                _server.StopListner();
            }
            catch { }
            _receiver.StopReceiving();
        }
    }
}
