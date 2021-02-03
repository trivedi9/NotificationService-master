/*
 *  File Name : NotifyObject.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/19/2010
 *  
 */
namespace CooperAtkins.Interface.NotifyCom
{
    using System.Text;
    using System.Collections;
    using System.Xml;
    /// <summary>
    /// class to construct notify object to be sent to notify engine
    /// </summary>
    public class NotifyObject : INotifyObject
    {
        private bool _twoWayCommunication = true;
   
        #region INotifyObject Members
        
   
        public bool TwoWayCommunication
        {
            get{return _twoWayCommunication;}
            set {_twoWayCommunication = value;}
        }
        /// <summary>
        /// data that is to be sent to notification device
        /// </summary>
        public object NotificationData
        {
            get;
            set;
        }
        /// <summary>
        /// type of notification device
        /// </summary>
        public string NotificationType
        {
            get;
            set;
        }
        /// <summary>
        /// configuration settings for the notification device
        /// </summary>
        public System.Collections.Hashtable NotifierSettings
        {
            get;
            set;
        }

        /*
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
        /// <summary>
        /// Prepare XML string to send other side (To notification engine)
        /// </summary>
        /// <returns></returns>
        public string GetXML()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<notification ack='{0}'>", TwoWayCommunication ? "true" : "false");
            sb.AppendFormat("<notificationData><![CDATA[{0}]]></notificationData>", NotificationData??string.Empty);
            sb.AppendFormat("<notificationType><![CDATA[{0}]]></notificationType>", NotificationType);
            sb.Append("<notificationSettings>");

            if (NotifierSettings != null)
            {
                foreach (DictionaryEntry dictEntry in NotifierSettings)
                {
                    sb.AppendFormat("<{0}><![CDATA[{1}]]></{0}>", dictEntry.Key, dictEntry.Value);
                }
            }

            sb.Append("</notificationSettings></notification>");

            return sb.ToString();
        }

        #endregion
        /// <summary>
        /// construct notify object for the given xml string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static INotifyObject Create(object data) {
            
            INotifyObject notifyObject = new NotifyObject();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(data.ToString());

            foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes) { 
                
                switch(xmlNode.Name)
                {
                    case "notificationData":
                        notifyObject.NotificationData = xmlNode.InnerText;
                        break;
                    case "notificationType":
                        notifyObject.NotificationType = xmlNode.InnerText;
                        break;
                    case "notificationSettings":
                        Hashtable settings = new Hashtable();
                        foreach (XmlNode setting in xmlNode.ChildNodes)
                        {
                            settings.Add(setting.Name, setting.InnerText);
                        }
                        notifyObject.NotifierSettings = settings;
                        break;
                }
                 
            }
            return notifyObject;
        }
    }
}
