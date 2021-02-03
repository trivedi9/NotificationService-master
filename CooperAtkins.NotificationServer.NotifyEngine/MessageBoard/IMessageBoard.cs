/*
 *  File Name : IMessageBoard.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/24/2010
 */
namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;

    public interface IMessageBoard
    {
        string Description { get; }
        string COMMSettings { get; set; }
        MessageBoardType BoardType { get; set; }
        string Name { get; set; }
        Int32 ID { set; get; }
        string LastError { get; set; }
        bool IsEnabled { get; set; }
        Int32 Port { get; set; }
        string IPAddress { get; set; }
        bool IsNetworkAttached { get; set; }
        bool IsGroup { get; set; }
        string SensorAlarmID { get; set; }
        bool? IsDynamicNotificationCleared { get; set; }
        string SensorFactoryID { get; set; }
        bool SetServerTime { get; set; }

        void DisplayMessage(string msg, int priority = 5);

       
                
    }
}
