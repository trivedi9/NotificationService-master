/*
 *  File Name : NotificationReceiver.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  
 */
namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Xml;
    using CooperAtkins.Interface.NotifyCom;

    
    public class NotificationReceiver : INotifyReceiver
    {
        private Notification _notification;
        public NotificationReceiver() {
            _notification = new Notification();
        }

        public void StopReceiving(){
            _notification.UnLoad();
        }

        /// <summary>
        /// Prepare INotifyObject and send invoke notification components
        /// </summary>
        /// <param name="data">XML data(string/XMLReader)
        /// </param>
        /*   Nodes - 
         *  <notification ack='true/false'>
            <notificationData> </notificationData>
            <notificationType> </notificationType>
         *  <notificationSettings> 
         *      <<Setting Name1>></<Setting Name1>>
         *      <<Setting Name2>></<Setting Name3>>
         *      <<Setting Name3>></<Setting Name4>>
         * </notificationSettings>
         * </notification>
        */
        public object Execute(object data) {
            return Execute(PrepareNotifyObject(data)).GetXML();
        }
        public NotifyComResponse Execute(INotifyObject notifyObject) {
            return _notification.SendNotification(notifyObject);
        }
        public INotifyObject PrepareNotifyObject(object data)
        {
            if (data is string || data is XmlReader)
            {
            }
            else
            {
                throw new ArgumentNullException("data parameter must be string or XmlReader type.");
            }

            INotifyObject notifyObject = NotifyObject.Create(data);
            return notifyObject;
        }
    }


  
}
