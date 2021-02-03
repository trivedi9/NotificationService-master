/*
 *  File Name : INotificationComposer.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  
 */
namespace CooperAtkins.Interface.NotificationComposer
{
    using CooperAtkins.Interface.NotifyCom;
    public interface INotificationComposer
    {
        INotifyObject[] Compose(CooperAtkins.Interface.Alarm.AlarmObject alarmObject);
        void Receive(Interface.NotifyCom.NotifyComResponse response, INotifyObject notifyObject);
    }
}
