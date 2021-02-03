
/*
 *  File Name : PopupNotificationComposer.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Text;
    using System.Web;
    using System.Collections;
    using System.Configuration;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.Generic;
    using CooperAtkins.NotificationClient.NotificationComposer.DataAccess;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.Interface.NotificationComposer;

    [Export(typeof(INotificationComposer))]
    public class PopupNotificationComposer : INotificationComposer
    {
        /*Prepare log content */
        private string _logContent = "";

        #region INotificationComposer Members

        public INotifyObject[] Compose(AlarmObject alarmObject)
        {
            NotifyPopupAddressList notificationPopupAddressList = null;

            string popupMessage = "";

            /*Notify List*/
            List<INotifyObject> notifyList = new List<INotifyObject>();

            /*Initialize NotifySettings*/
            NotificationStyle notificationStyle = new NotificationStyle();

            if (alarmObject.IsMissCommNotification)
                _logContent = "Missed Communication";
            else
                _logContent = "SensorID: " + alarmObject.UTID.ToStr() + " SensorAlarmID: " + alarmObject.SensorAlarmID.ToStr();

            /*Check whether server popup or remote popup*/
            if (alarmObject.IsServerPopup)
            {
                /*Write Log: 
                * Started Server popup Notification
                * Reached Server popup Notification Composer*/
                LogBook.Write("*** Started Composing Server popup Notification for " + _logContent + "***");

                /*Get custom message for missed communication*/
                if (alarmObject.IsMissCommNotification)
                {
                    popupMessage = "Missed Communication" + " [" + alarmObject.MissedCommSensorCount + "] sensors";
                }
                else
                {
                    /*Get popup message*/
                    popupMessage = notificationStyle.GetFormatString(alarmObject, 1, "Popup");

                    /*Substitute message parameters*/
                    popupMessage = notificationStyle.SubstituteFormatString(popupMessage, alarmObject);
                }

                /*Server popup*/
                NotifyPopupAddress notifyPopupAddress = new NotifyPopupAddress();
                notifyPopupAddress.NetSendTo = ConfigurationManager.AppSettings.Get("NetSendTo").ToStr();
                notifyPopupAddress.Name = ConfigurationManager.AppSettings.Get("NetSendFromName").ToStr();

                /*Get Notification Object*/
                INotifyObject notifyObject = GetNotifyObject(alarmObject, popupMessage, notifyPopupAddress);

                if (notifyList.Count > 0)
                    /*Write Log
                    * Sending Server Popup notification data to Notification Engine*/
                    LogBook.Write(_logContent + " Sending notification data to Server Popup  Notification Engine.");

                notifyList.Add(notifyObject);
            }
            else
            {
                /*Write Log: 
                  * Started Server popup Notification
                  * Reached Server popup Notification Composer*/
                LogBook.Write("*** Started Composing Remote popup Notification for " + _logContent + "***");

                /*Get custom message for missed communication*/
                if (alarmObject.IsMissCommNotification)
                {
                    popupMessage = "Missed Communication" + " [" + alarmObject.MissedCommSensorCount + "] sensors";
                }
                else
                {
                    /*Get NetSend message*/
                    popupMessage = notificationStyle.GetFormatString(alarmObject, 1, "NetSend");

                    /*Notification Tree*/
                    string notifyTree = notificationStyle.GetFormatString(alarmObject, 1, "NotifyTree");

                    /*Format Subject*/
                    string netSendMsg = notificationStyle.SubstituteFormatString(popupMessage, alarmObject);

                    popupMessage = netSendMsg;

                    /*Format Notify Tree*/
                    string netTreeMsg = notificationStyle.SubstituteFormatString(notifyTree, alarmObject);

                    notifyTree = netTreeMsg;

                    if (notifyTree != string.Empty)
                    {
                        popupMessage = popupMessage + "%%NOTIFYTREE%%" + notifyTree;
                    }
                }

                /*Get Notification Popup Address List*/
                notificationPopupAddressList = GetNotifyPopupAddressList(alarmObject);

                if (notificationPopupAddressList.Count != 0)
                {
                    /*Get Popup Notification list*/
                    notifyList = GetNotificationList(notificationPopupAddressList, alarmObject, popupMessage);
                }
                else
                {
                    /*Log when we don't have Popup address to notify*/
                    LogBook.Write(_logContent + " Error: Missing entry in NotifyEmails/Groups");
                }

                if (notifyList.Count > 0)
                    /*Write Log
                    * Sending Remote Popup notification data to Notification Engine*/
                    LogBook.Write(_logContent + " Sending notification data to Remote Popup  Notification Engine.");
            }
            return notifyList.ToArray();
        }

        /// <summary>
        /// Receive response and record notification
        /// </summary>
        /// <param name="response"></param>
        /// <param name="notifyObject"></param>
        public void Receive(NotifyComResponse response, INotifyObject notifyObject)
        {
            /*Create notification style object*/
            NotificationStyle notificationStyle = new NotificationStyle();

            /*Check the response object*/
            if (response != null)
            {
                /*Record notification information.*/
                notificationStyle.RecordNotification(response.ResponseContent.ToStr().Replace("127.0.0.1", "server"), notifyObject.NotifierSettings["NotificationID"].ToInt(), 0, (response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL), NotifyTypes.POPUP);

                /*Log if Popup sent*/
                LogBook.Write(_logContent + " Popup Response: " + response.ResponseContent.ToStr());
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private NotifyObject GetNotifyObject(AlarmObject alarmObject, string popupMessage, NotifyPopupAddress notifyPopupAddress)
        {
            string alertTime = "", location = "", temperatureReading = "", notificationData = "";
            /*Initialize NotifyObject*/
            NotifyObject notifyObject = new NotifyObject();

            /*Notification settings*/
            NotificationStyle notificationStyle = new NotificationStyle();

            /*Alert Time*/
            alertTime = alarmObject.AlarmTime.ToLocalTime().ToString();
            //Common.LocalTimeToUTC(alarmObject.AlarmTime, Common.TZ_OFFSET).ToStr();
            alertTime = (alertTime == string.Empty ? DateTime.Now.ToStr() : alertTime);

            /*Location*/
            location = alarmObject.ProbeName.ToStr() + (alarmObject.GroupName.ToStr() != string.Empty ? ("(" + alarmObject.GroupName + ")") : "");

            /*HOT COLD*/
            //Check for contact sensor
            if (AlarmHelper.IsContactSensor(alarmObject.SensorType))
                temperatureReading = (alarmObject.Value == 0) ? "CLOSED" : "OPEN";
            else
                temperatureReading = (alarmObject.Value < alarmObject.AlarmMinValue) ? "LOW" : "HIGH";

            /*Building the alert message data*/
            notificationData = "<alert><msg>" + HtmlEncode(popupMessage) + "</msg>" +
                              "<time_local>" + HtmlEncode(alertTime) + "</time_local>"
                                + "<time_gmt>" + HtmlEncode(alarmObject.AlarmTime.ToStr()) + "</time_gmt>"
                                + "<loc>" + HtmlEncode((alarmObject.GroupName.ToStr())) + "</loc>"
                                + "<name>" + HtmlEncode((alarmObject.ProbeName.ToStr())) + "</name>"
                                + "<type>" + HtmlEncode((alarmObject.SensorType.ToStr())) + "</type>"
                                + "<class>" + HtmlEncode((alarmObject.SensorClass.ToStr())) + "</class>"
                                + "<alertType>" + HtmlEncode((temperatureReading)) + "</alertType>"
                                + "</alert>";

            string temp = "00000000";
            int strLength = temp.Length - notificationData.Length.ToString().Length;
            notificationData = temp.Substring(0, strLength) + notificationData.Length + notificationData;
            /*Assign values to Notification settings*/
            Hashtable notificationSettings = new Hashtable();

            /*Remote Port*/
            notificationSettings.Add("RemotePort", ConfigurationManager.AppSettings["PortNumber"]);

            /*Remote Host*/
            notificationSettings.Add("RemoteHost", notifyPopupAddress.NetSendTo);

            /*Name that was configured in temp trak application*/
            notificationSettings.Add("Name", notifyPopupAddress.Name);

            /*Notification ID*/
            notificationSettings.Add("NotificationID", alarmObject.NotificationID);

            /*Notification settings*/
            notifyObject.NotifierSettings = notificationSettings;

            /*Pop up message*/
            notifyObject.NotificationData = notificationData;

            /*Set Notification Type*/
            notifyObject.NotificationType = (alarmObject.IsServerPopup) ? "ServerPopup" : "RemotePopup";

            return notifyObject;
        }

        /// <summary>
        /// Format to XML  
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string HtmlEncode(string text)
        {
            char[] chars = HttpUtility.HtmlEncode(text).ToCharArray();
            StringBuilder result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (char c in chars)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        /// <summary>
        /// Get Notification E-Mail Object
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private NotifyPopupAddressList GetNotifyPopupAddressList(Interface.Alarm.AlarmObject alarmObject)
        {
            NotifyPopupAddressList notificationPopupAddressList = null;
            try
            {
                /*Fetch Popup Address list based on Notification Profile ID*/
                notificationPopupAddressList = new NotifyPopupAddressList();
                Criteria criteria = new Criteria();
                criteria.ID = GetPopupNotificationID(alarmObject.NotifyProfileID);//Replace with 
                notificationPopupAddressList.Load(criteria);
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notificationPopupAddressList, this);

                /*Write exception log*/
                LogBook.Write(_logContent + " Error has occurred while fetching Popup address list from 'GenNotifyEmails' table", ex, "CooperAtkins.NotificationClient.NotificationComposer.EmailNotificationComposer");
            }
            return notificationPopupAddressList;
        }

        /// <summary>
        /// Get Notification ID based on Notification Type
        /// </summary>
        /// <param name="notifyProfileID"></param>
        /// <returns></returns>
        private int GetPopupNotificationID(int notifyProfileID)
        {
            NotificationProfile notificationProfile = null;
            int popupNotificationID = 0;
            try
            {
                notificationProfile = new NotificationProfile();
                notificationProfile.NotifyProfileID = notifyProfileID;
                notificationProfile = notificationProfile.Execute();
                popupNotificationID = notificationProfile.NetSendNotifyID;
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notificationProfile, this);

                /*Write exception log*/
                LogBook.Write(_logContent + " Error has occurred while fetching EmailNotifyID ", ex, "CooperAtkins.NotificationClient.NotificationComposer.EmailNotificationComposer");
            }
            return popupNotificationID;
        }

        /// <summary>
        /// Get Popup notification list
        /// </summary>
        /// <param name="notifyEmailAddressList"></param>
        /// <returns></returns>
        private List<INotifyObject> GetNotificationList(NotifyPopupAddressList notificationPopupAddressList, AlarmObject alarmObject, string popupMessage)
        {
            List<INotifyObject> notifyList = new List<INotifyObject>();
            NotificationStyle notificationStyle = new NotificationStyle();

            foreach (NotifyPopupAddress notifyPopupAddress in notificationPopupAddressList)
            {
                /*Check for Notification Popup Address and exit the sending process, if NotificationEmailAddress object is NULL*/
                if (notifyPopupAddress != null)
                {
                    /*Get Notification Object*/
                    INotifyObject notifyObject = GetNotifyObject(alarmObject, popupMessage, notifyPopupAddress);

                    //Add notification object to array list
                    notifyList.Add(notifyObject);

                }
                else
                {
                    /*Record notification if Popup Address parameters are not supplied properly*/
                    notificationStyle.RecordNotification("", alarmObject.NotificationID, 0,NotifyStatus.FAIL, NotifyTypes.POPUP);
                }
            }
            return notifyList;
        }


    }
}
