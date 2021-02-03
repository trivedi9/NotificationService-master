/*
 *  File Name : ScriptNotificationComposer.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/30/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
        using System.Configuration;
    using System.ComponentModel.Composition;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Interface.NotificationComposer;
    using Microsoft.VisualBasic;
   

    [Export(typeof(INotificationComposer))]
    public class ScriptNotificationComposer : INotificationComposer
    {
        /*Prepare log content */
        private string _logContent = "";

        #region INotificationComposer Members
        public INotifyObject[] Compose(AlarmObject alarmObject)
        {
            List<INotifyObject> notifyList = new List<INotifyObject>();

            

            /*Write Log: 
             * Started Script Notification
             * Reached Script Notification Composer*/

            if (alarmObject.IsMissCommNotification)
                _logContent = "Missed Communication";
            else
                _logContent = "SensorID: " + alarmObject.UTID.ToStr() + " SensorAlarmID: " + alarmObject.SensorAlarmID.ToStr();
            LogBook.Write("*** Started Composing Script Notification for " + _logContent + "***");

            /*Get Notification Object*/
            INotifyObject notifyObject = GetNotifyObject(alarmObject);

            /*Add to Notification List*/
            notifyList.Add(notifyObject);

            if (notifyList.Count > 0)
                /*Write Log
                 * Sending Script notification data to Notification Engine*/
                LogBook.Write(_logContent + " Sending notification data to Script  Notification Engine.");

            return notifyList.ToArray();
        }

        public void Receive(NotifyComResponse response, INotifyObject notifyObject)
        {
            NotificationStyle notificationStyle = new NotificationStyle();

            /*Write Log
             Response received from Notification Engine*/
            LogBook.Write(_logContent + " Response received from Script Notification Engine.");

            /*Check the response object*/
            if (response != null)
            {
                /*Record notification indicating that the script executed successfully*/
                notificationStyle.RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(), 0, (response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL), NotifyTypes.SCRIPT);

                /*Log response content*/
                LogBook.Write(_logContent + " Script Response: " + response.ResponseContent.ToStr());
            }
        }

        #endregion

        /// <summary>
        /// Get alarm script arguments
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private string GetScriptArguments(AlarmObject alarmObject)
        {
            string scriptArgs = Strings.Chr(34) + alarmObject.FactoryID + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.UTID + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.Probe + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.AlarmTime + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.AlarmMinValue + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.AlarmMaxValue + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.Value + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.DisplayValue + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.Threshold + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.CurrentAlarmMinutes + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.AlarmStartTime + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.SensorType + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.GroupName + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.AlarmID + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.NotificationID + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.ProbeName + Strings.Chr(34) + " " + Strings.Chr(34) + alarmObject.AlarmType + Strings.Chr(34) + " " + Strings.Chr(34) + (alarmObject.IsEscalationNotification | alarmObject.IsFailsafeEscalationNotification ? 1 : 0) + Strings.Chr(34) + " " + Strings.Chr(34) + 0 + Strings.Chr(34) + " "; //Replace OUID  with original value
            return scriptArgs;
        }

        /// <summary>
        /// Get notification object
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private NotifyObject GetNotifyObject(AlarmObject alarmObject)
        {
            string scriptArgs = GetScriptArguments(alarmObject);
            string errorMsg = string.Empty;
            string scriptName = ConfigurationManager.AppSettings.Get("ScriptPath").ToStr();
            NotifyObject notifyObject = new NotifyObject();

            NotificationStyle notificationStyle = new NotificationStyle();

            /*Set Notification Type*/
            notifyObject.NotificationType = "Switch";

            /*Notification Data*/
            notifyObject.NotificationData = "";

            Hashtable notificationSettings = new Hashtable();
            /*Script Arguments*/
            notificationSettings.Add("ScriptArgs", scriptArgs);

            /*Script Name*/
            notificationSettings.Add("ScriptName", scriptName);

            /*Notification ID*/
            notificationSettings.Add("NotificationID", alarmObject.NotificationID);

            /*Notification Settings*/
            notifyObject.NotifierSettings = notificationSettings;

            if (scriptName == string.Empty)
            {
                errorMsg = string.Format(ErrorMessages.ScriptComposer_InvalidScriptFileName, scriptName);

                /*If the script file name is empty record notification.*/
                notificationStyle.RecordNotification(errorMsg, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.SCRIPT);

                /*Log error*/
                LogBook.Write(_logContent + " *** Skipping custom alert script - no script found");
            }
            else if (!File.Exists(scriptName)) /*Check weather the script file exist or not*/
            {

                errorMsg = string.Format(ErrorMessages.ScriptComposer_ScriptFileNotFound, scriptName);

                /*If file doesn't exist in the specified path record notification*/
                notificationStyle.RecordNotification(errorMsg, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.SCRIPT);

                /*Log if script file not found*/
                LogBook.Write(_logContent + " *** Skipping custom alert script - file not found: ");
            }

            return notifyObject;
        }
    }
}
