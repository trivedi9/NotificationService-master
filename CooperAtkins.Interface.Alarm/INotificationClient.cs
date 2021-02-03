/*
 *  File Name : NotificationClient.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  
 */
namespace CooperAtkins.Interface.Alarm
{
    /// <summary>
    /// INotificationClient Interface to expose send methods
    /// </summary>
    public interface INotificationClient
    {
        void Send(AlarmObject alarmObject, string[] notificationTypes);
        void Send(AlarmObject alarmObject);
    }
}
