/*
 *  File Name : INotificationProtocolClient.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/26/2010
 *  
 */
using System;
namespace CooperAtkins.Interface.NotifyCom
{
    public interface INotificationChannelClient : INotificationChannel
    {
        /// <summary>
        /// Connect to end point
        /// </summary>
        /// <returns></returns>
        bool Connect();
        /// <summary>
        /// Send data to server
        /// </summary>
        /// <param name="data"></param>
        void Send(string data);
        /// <summary>
        /// Receive data 
        /// </summary>
        /// <param name="data"></param>
        void OnReceive(Action<string, string> receiveAction);
    }

}
