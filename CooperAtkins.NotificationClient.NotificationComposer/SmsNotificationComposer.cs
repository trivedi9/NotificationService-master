/*
 *  File Name : SMSNotificationComposer.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 12/29/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.ComponentModel.Composition;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Interface.NotificationComposer;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.NotificationClient.NotificationComposer.DataAccess;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.NotificationClient.Generic;
    using EnterpriseModel.Net;
    [Export(typeof(INotificationComposer))]
    public class SmsNotificationComposer : INotificationComposer
    {
        

        #region INotificationComposer Members

        public INotifyObject[] Compose(AlarmObject alarmObject)
        {
            GenStoreInfo genStoreInfo = null;
            NotifyMobileList notifyMobileList = null;
            List<INotifyObject> notifyList = new List<INotifyObject>();
            NotificationStyle notificationStyle = new NotificationStyle();
            LogBook.Write("Executing Compose method");
            

            /*Get GenStore Information*/
            genStoreInfo = GenStoreInfo.GetInstance();

            try
            {
                //If there's no record in the database, Execute method will return null 
                if (genStoreInfo != null)
                {
                    /*Record notification if SMTP settings are not supplied properly*/
                    //if (genStoreInfo.SmtpServer.ToStr() != string.Empty && genStoreInfo.SmtpSendMethod != 0)
                    //if ((genStoreInfo.ToPhoneNumber == string.Empty && genStoreInfo.SmtpAuthDomain == string.Empty) && genStoreInfo.SmtpSendMethod == 0)
                    //{
                    //    /*SMTP server not defined*/
                    //    LogBook.Write("Error: No SMTP Server has been defined");

                    //    /*Record notification if SMTP parameters are not supplied properly*/
                    //    notificationStyle.RecordNotification(ErrorMessages.SmsComposer_SMTPParmsNotSupplied, _alarmObject.NotificationID, NotifyStatus.FAIL, NotifyTypes.SMS);

                    //}
                    //else
                    //{
                        LogBook.Write("Fetching notify Mobiles List");
                        /*Get all the sms addresses that are configured for notification*/
                        notifyMobileList = GetNotifyMoibilesList(alarmObject.NotifyProfileID);
                        LogBook.Write("Notify Mobile List Count: " + notifyMobileList.Count.ToString());
                        if (notifyMobileList.Count != 0)
                        {
                            LogBook.Write("Constructing notify list objects");
                            /*Construct a notification object for each sms number and add to the notification list*/
                            notifyList = GetNotificationList(notifyMobileList, alarmObject, genStoreInfo);


                            for (int index = notifyList.Count - 1; index >= 0; index--)
                            {
                                INotifyObject notifyObject = notifyList[index];
                                SmsHelper.SmsQueue.Add(notifyObject);
                                notifyList.Remove(notifyObject);
                                new NotificationStyle().RecordNotification("Sms queued: " + notifyObject.NotifierSettings["ToPhoneNumber"].ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(),0, NotifyStatus.STATUS, NotifyTypes.SMS);
                            }


                            LogBook.Write("Notify list objects count: " + notifyList.Count);

                        }
                        else
                        {
                            /*Log when we don't have sms address to notify*/
                            LogBook.Write("Error: Missing entry in NotifySms/Groups");
                        }
                    //}
                }
                else
                {
                    /*Record notification if genStore parameters are not supplied properly.*/
                    notificationStyle.RecordNotification(ErrorMessages.SMSComposer_InvalidSettings, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.SMS);

                    /*Log if genStore information is empty*/
                    LogBook.Write("Error:  Missing GenStores table record");
                }
            }
            catch (Exception ex)
            {
                /*Write exception log*/
                LogBook.Write(ex, "CooperAtkins.NotificationClient.NotificationComposer.SmsNotificationComposer");
            }

            /*Send notification list to notification engine*/
            return notifyList.ToArray();
        }

        /// <summary>
        /// Receive notification response.
        /// </summary>
        /// <param name="response"></param>
        public void Receive(NotifyComResponse response, INotifyObject notifyObject)
        {
            NotificationStyle notificationStyle = new NotificationStyle();
            /*Check the response object*/
            if (response != null)
            {
                /*Record notification information.*/
                notificationStyle.RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(),0, (response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL), NotifyTypes.SMS);

                if (response.IsError == false)
                {
                    /*Log if sms send to particular to address*/
                    LogBook.Write("SMS sent to: " + notifyObject.NotifierSettings["ToPhoneNumber"].ToStr());
                }
                else
                {
                    /*Log when sending sms failed*/
                    LogBook.Write("Error has occurred while sending SMS");
                    /*Log response content*/
                    LogBook.Write(response.ResponseContent.ToStr());
                }
            }
        }

        #endregion

        /// <summary>
        /// Get E-mail Notification ID
        /// </summary>
        /// <param name="notifyProfileID"></param>
        /// <returns></returns>
        private int GetMobileNotificationID(int notifyProfileID)
        {
            NotificationProfile notificationProfile = null;
            int smsNotificationID = 0;
            try
            {
                LogBook.Write("Fetching Notify ID");
                /*Create NotificationProfile instance*/
                notificationProfile = new NotificationProfile();
                /*Send criteria notifyProfileID to get the Notification ID*/
                notificationProfile.NotifyProfileID = notifyProfileID;
                /*Execute*/
                notificationProfile = notificationProfile.Execute();
                /*Assign the SmsNotifyID to local variable smsNotificationID*/
                smsNotificationID = notificationProfile.SMSNotifyID;
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notificationProfile, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while fetching SMSNotifyID ", ex, "CooperAtkins.NotificationClient.NotificationComposer.SmsNotificationComposer");
            }
            finally
            {
                /*Dispose the notificationProfile object*/
                notificationProfile.Dispose();
            }
            return smsNotificationID;
        }

        /// <summary>
        /// Get Notification E-Mail Address list based on Notification Profile ID
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private NotifyMobileList GetNotifyMoibilesList(int notifyProfileID)
        {
            NotifyMobileList notifyMobileList = null;
            try
            {
                LogBook.Write("Fetching Mobile notify ID list");
                /*Create NotifySmsAddressList object*/
                notifyMobileList = new NotifyMobileList();
                /*Create Criteria object*/
                Criteria criteria = new Criteria();
                /*Get the Sms notificationId and assign to criteria.ID*/
                criteria.ID = GetMobileNotificationID(notifyProfileID);
                /*Gets list of notification sms addresses*/
                notifyMobileList.Load(criteria);

                LogBook.Write("Notify Mobile ID list count: " + notifyMobileList.Count);
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notifyMobileList, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while fetching mobile numbers list from 'GenNotifySms' table", ex, "CooperAtkins.NotificationClient.NotificationComposer.SMSNotificationComposer");
            }
            finally
            {
                notifyMobileList.Dispose();
            }
            return notifyMobileList;
        }

        /// <summary>
        /// Get Sms notification list
        /// </summary>
        /// <param name="notifySmsAddressList"></param>
        /// <returns></returns>
        private List<INotifyObject> GetNotificationList(NotifyMobileList notifyMobileList, AlarmObject alarmObject, GenStoreInfo genStoreInfo)
        {
            List<INotifyObject> notifyList = new List<INotifyObject>();
            NotificationStyle notificationStyle = new NotificationStyle();

            foreach (NotifyMobiles notifyMobiles in notifyMobileList)
            {
                /*Check for Notification Sms Address and exit the sending process, if NotificationSmsAddress object is NULL*/
                if (notifyMobiles != null)
                {
                    /*Get Notification Object*/
                    INotifyObject notifyObject = GetNotifyObject(alarmObject, genStoreInfo, notifyMobiles);

                    if (notifyObject.NotifierSettings["ToPhoneNumber"].ToStr() == string.Empty || notifyObject.NotifierSettings["COMSettings"].ToStr() == string.Empty)
                    {
                        LogBook.Write("Missing mobile number or COM Settings");
                        /*Record notification if Sms parameters are not supplied properly*/
                        notificationStyle.RecordNotification(ErrorMessages.SMSComposer_InvalidSettings, notifyObject.NotifierSettings["NotificationID"].ToInt(),0, NotifyStatus.FAIL, NotifyTypes.SMS);
                    }
                    else
                    {
                        //Add notification object to array list
                        notifyList.Add(notifyObject);
                    }
                }
                else
                {
                    /*Record notification if Sms parameters are not supplied properly*/
                    notificationStyle.RecordNotification(ErrorMessages.SMSComposer_InvalidSettings, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.SMS);
                }
            }
            return notifyList;
        }

        /// <summary>
        /// Create Notification Object
        /// </summary>
        /// <param name="genStoreDAO"></param>
        /// <returns></returns>
        private NotifyObject GetNotifyObject(Interface.Alarm.AlarmObject alarmObject, GenStoreInfo genStoreInfo, NotifyMobiles notifyMobiles)
        {
            string toPhoneNumber = string.Empty;
            string toName = string.Empty;
            string message = string.Empty;
            NotificationStyle notificationStyle = new NotificationStyle();

            
            //*Initialize NotifyObject*//
            NotifyObject notifyObject = new NotifyObject();
            
            //*Mobile Number*//
            toPhoneNumber = notifyMobiles.MobileNumber;

            //*Mobile name*//
            toName = notifyMobiles.Name;

            //* Message *//
            message = alarmObject.Value.ToString();

            //if message length is greater than zero, get the format string for the message//
            if (message.Length > 0)
                message = notificationStyle.GetFormatString(alarmObject, 1, "MessageBoard");

            //replace line breaks with spaces
            message = message.Replace("\\n", " ");

            //substitute actual values for the format strings using the alarm object
            message = notificationStyle.SubstituteFormatString(message, alarmObject);

            ///*Assigning values to notification object*/
            notifyObject.NotificationType = "SMS";

            ///*Assign values to Notification settings*/
            Hashtable notificationSettings = new Hashtable();
            notificationSettings.Add("COMPort", genStoreInfo.MobileCOMPort);
            notificationSettings.Add("COMSettings", genStoreInfo.MobileCOMSettings);
            notificationSettings.Add("PIN1", genStoreInfo.PIN1);
            notificationSettings.Add("PIN2", genStoreInfo.PIN2);
            notificationSettings.Add("ServiceCenterNumber", genStoreInfo.SMSProviderServiceCenter);
            notificationSettings.Add("ToPhoneNumber", toPhoneNumber);
            notificationSettings.Add("FrqBand", genStoreInfo.FrequencyBand);
            notificationSettings.Add("ToName", toName);
            notificationSettings.Add("NotificationID", alarmObject.NotificationID);
            notificationSettings.Add("AttemptCount", "0");
            notifyObject.NotifierSettings = notificationSettings;
            ///* Notification Data */
            notifyObject.NotificationData = message;

            return notifyObject;
        }
    }
}
