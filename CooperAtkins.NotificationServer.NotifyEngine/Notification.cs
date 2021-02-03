/*
 *  File Name : Notification.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 *  
 */
namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using CooperAtkins.Interface.NotifyCom;

    public class Notification
    {
        private ImportNotificationComponents _notificationComponents;
        public Notification() {
            _notificationComponents = new ImportNotificationComponents();
            _notificationComponents.Import();
        }
        public void UnLoad() {
            _notificationComponents.UnLoad();
        }

        /// <summary>
        /// Send notification 
        /// </summary>
        /// <param name="notifyObject"></param>
        public NotifyComResponse SendNotification(INotifyObject notifyObject)
        {
           return _notificationComponents.Run(notifyObject);
        }
    }
}
