/*
 *  File Name : INotificationProtocolServer.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/26/2010
 *  
 */
namespace CooperAtkins.Interface.NotifyCom
{
    using System;
    public interface INotificationChannelServer : INotificationChannel
    {
        /// <summary>
        /// Start listener
        /// </summary>
        void StartListner();
        /// <summary>
        /// Stop Listener
        /// </summary>
        void StopListner();
        /// <summary>
        /// Receive data 
        /// </summary>
        /// <param name="data"></param>
        void OnReceive(Func<string, string, string> receiveAction);
    }
}
