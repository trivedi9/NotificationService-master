/*
 *  File Name : SwitchNotificationComposer.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 11/30/2010
 */
namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using EnterpriseModel.Net;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;
    using System.ComponentModel.Composition;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.Interface.NotificationComposer;


    [Export(typeof(INotificationComposer))]
    public class SwitchNotificationComposer : INotificationComposer
    {

        /// <summary>
        /// method to compose notify object to be used in NotifyCom 
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        public INotifyObject[] Compose(AlarmObject alarmObject)
        {

            List<INotifyObject> notifyObjectList = null;
            try
            {
                //if the dynamic notification is already cleared , no need to send to Notify objects list to notify engine
                if (alarmObject.IsSwitchNotificationClearProcessStarted == true)
                {
                    return new List<INotifyObject>().ToArray();
                }
                //if IsDynamicNotificationCleared is true we need to set the below flag in order skip notifications
                //received from any escalations while the clear process is going on
                if (alarmObject.IsDynamicNotificationCleared)
                {
                    alarmObject.IsSwitchNotificationClearProcessStarted = true;
                }

                notifyObjectList = new List<INotifyObject>();

                //Get Switch notifyID using NotifyProfileID
                int switchNotifyID = GetSwitchNotifyID(alarmObject.NotifyProfileID, alarmObject.NotificationID);

                //Get relay switch configuration object
                RelaySwitchConfig relaySwitchConfig = GetRelaySwitchConfig(switchNotifyID);

                //Populate the relaySwitch config settings into notification settings, for use in Switch Notify Com
                NotifyObject notifyObject = new NotifyObject();

                //assign the bit mask value set in the escalation profiles and sensor attributes
                notifyObject.NotificationData = alarmObject.SwitchBitmask;

                //set the notification type
                notifyObject.NotificationType = "RELAYSWITCH";

                //assign the switch configurations to notified settings
                Hashtable ht = GetNotificationSettings(relaySwitchConfig);
                ht["IsDynamicNotificationCleared"] = alarmObject.IsDynamicNotificationCleared;
                ht["SensorAlarmID"] = alarmObject.SensorAlarmID;
                ht["FactoryID"] = alarmObject.FactoryID;
                ht["NotificationID"] = alarmObject.NotificationID;

                notifyObject.NotifierSettings = ht;

                //add notify object to list
                notifyObjectList.Add(notifyObject);

            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.NotificationComposer.SwitchNotificationComposer");
            }
            //return the notify objects to Notify Com
            return notifyObjectList.ToArray();


        }
        /// <summary>
        /// method to handle response received after invoking the Notify object
        /// </summary>
        /// <param name="response"></param>
        /// <param name="notifyObject"></param>
        public void Receive(NotifyComResponse response, INotifyObject notifyObject)
        {
            try
            {
                /*Check the response object*/
                if (response != null)
                {
                    NotificationStyle notificationStyle = new NotificationStyle();
                    if (response.IsError == false)
                    {
                        /*Record notification information.*/
                        notificationStyle.RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(),0, response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL, NotifyTypes.SWITCH);
                    }
                    else
                    {
                        notificationStyle.RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(), 0, NotifyStatus.FAIL, NotifyTypes.SWITCH);
                        //Write Log
                        LogBook.Write(response.ResponseContent.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.NotificationComposer.SwitchNotificationComposer");
            }
        }

        #region HelperMethods
        /// <summary>
        /// Method to get the switch id based on Notify Profile ID
        /// </summary>
        /// <param name="notifyProfileID"></param>
        /// <param name="notificationID"></param>
        /// <returns></returns>
        private int GetSwitchNotifyID(int notifyProfileID, int notificationID)
        {
            NotificationProfile notificationProfile = new NotificationProfile();
            NotificationStyle notificationStyle = new NotificationStyle();
            try
            {
                notificationProfile.NotifyProfileID = notifyProfileID;
                notificationProfile.Execute();
                //if (notificationProfile.SwitchNotifyID == 0)
                //    notificationStyle.RecordNotification("Relay switch not configured for this notification", notificationID, NotifyStatus.FAIL, NotifyTypes.SWITCH);
            }
            catch (Exception ex)
            {
                LogBook.Write("   *** Error in GetSwitchNotifyID method:" + ex.Message);
            }
            return notificationProfile.SwitchNotifyID;
        }
        /// <summary>
        /// method to get the relay switch configuration
        /// </summary>
        /// <param name="switchNotifyID"></param>
        /// <returns></returns>
        private RelaySwitchConfig GetRelaySwitchConfig(int switchNotifyID)
        {
            RelaySwitchConfig relaySwitchConfig = new RelaySwitchConfig();
            using (RelaySwitchConfigContext context = new RelaySwitchConfigContext())
            {
                relaySwitchConfig = context.GetEntity(new Criteria()
                {
                    ID = switchNotifyID
                });
            }

            return relaySwitchConfig;
        }
        /// <summary>
        /// method to set relay switch configuration details to notifications settings hashtable
        /// </summary>
        /// <param name="relaySwitchConfig"></param>
        /// <returns></returns>
        private Hashtable GetNotificationSettings(RelaySwitchConfig relaySwitchConfig)
        {
            Hashtable notificationSettings = new Hashtable();
            try
            {
                notificationSettings.Add("NotifyID", relaySwitchConfig.NotifyID);
                notificationSettings.Add("Name", relaySwitchConfig.Name);
                notificationSettings.Add("SwitchInfo", relaySwitchConfig.SwitchInfo);
                notificationSettings.Add("IsEnabled", relaySwitchConfig.IsEnabled);
            }
            catch (Exception ex)
            {
                LogBook.Write("   *** Error in GetNotificationSettings method:" + ex.Message);
            }
            return notificationSettings;
        }
        #endregion
    }
}
