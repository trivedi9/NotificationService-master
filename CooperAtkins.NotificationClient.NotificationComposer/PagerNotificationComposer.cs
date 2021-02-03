/*
 *  File Name : PagerNotificationComposer.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Interface.NotificationComposer;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.NotificationClient.NotificationComposer.DataAccess;
    using EnterpriseModel.Net;
    using CooperAtkins.NotificationClient.Generic;

    [Export(typeof(INotificationComposer))]
    public class PagerNotificationComposer : INotificationComposer
    {
        /*Prepare log content */
        private string _logContent = "";

        #region INotificationComposer Members

        public INotifyObject[] Compose(AlarmObject alarmObject)
        {
            NotifyPagerAddressList notifyPagerAddressList = null;
            List<INotifyObject> notifyObjectList = null;

            if (alarmObject.IsMissCommNotification)
                _logContent = "Missed Communication";
            else
                _logContent = "SensorID: " + alarmObject.UTID.ToStr() + " SensorAlarmID: " + alarmObject.SensorAlarmID.ToStr();

            /*Write Log: 
             * Started Pager Notification
             * Reached Pager Notification Composer*/
            LogBook.Write("*** Started Composing Pager Notification for " + _logContent + "***");

            try
            {

                /*Get Pager Address List*/
                notifyPagerAddressList = GetPagerAddressList(alarmObject);
                LogBook.Debug("NotifyPagerAddressList count: " + notifyPagerAddressList.Count.ToStr());
                /*Get notification List*/
                notifyObjectList = GetNotificationList(notifyPagerAddressList, alarmObject);
                LogBook.Debug("NotifyObjectList count: " + notifyObjectList.Count.ToStr());
            }
            catch (Exception ex)
            {
                /*Write exception log*/
                if (_logContent.Trim() != string.Empty)
                    LogBook.Write(_logContent);
                LogBook.Write(ex, "CooperAtkins.NotificationClient.NotificationComposer.PagerNotificationComposer");

            }

            if (notifyObjectList.Count > 0)
                /*Write Log
                 * Sending Pager notification data to Notification Engine*/
                LogBook.Write(_logContent + " Sending notification data to Pager Notification Engine.");


            for (int index = notifyObjectList.Count - 1; index >= 0; index--)
            {
                INotifyObject notifyObject = notifyObjectList[index];
                /*if the pager type is digital then add it to the queue, removing from the current list (queue will be processed separately)*/
                if (notifyObject.NotifierSettings["DeliveryMethod"].ToInt() != 1)
                {
                    DigitalPagerHelper.DigitalPagerQueue.Add(notifyObject);
                    notifyObjectList.Remove(notifyObject);
                    new NotificationStyle().RecordNotification("Page queued: " + notifyObject.NotifierSettings["ToAddress"].ToStr() + ", Message:" + notifyObject.NotifierSettings["PagerMessage"].ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(), 0, NotifyStatus.STATUS, NotifyTypes.PAGER);
                }
            }

            return notifyObjectList.ToArray();
        }

        public void Receive(NotifyComResponse response, INotifyObject notifyObject)
        {
            NotificationStyle notificationStyle = new NotificationStyle();

            /*Write Log
             Response received from Notification Engine*/
            LogBook.Write(_logContent + " Response received from Pager Notification Engine.");

            /*Check the response object*/
            if (response != null)
            {
                /*Record notification information.*/
                notificationStyle.RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(),0, (response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL), NotifyTypes.PAGER);

                /*Log response content*/
                LogBook.Write(_logContent + " Pager Response: " + response.ResponseContent.ToStr());
            }
        }

        #endregion

        /// <summary>
        /// Get Pager Notification Address List
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private NotifyPagerAddressList GetPagerAddressList(Interface.Alarm.AlarmObject alarmObject)
        {
            NotifyPagerAddressList notifyPagerAddressList = null;
            try
            {
                /*Fetch Email Address list based on Notification Profile ID*/
                notifyPagerAddressList = new NotifyPagerAddressList();
                Criteria criteria = new Criteria();
                criteria.ID = GetPagerNotificationID(alarmObject.NotifyProfileID);//Replace with 
                notifyPagerAddressList.Load(criteria);
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notifyPagerAddressList, this);

                /*Write exception log*/
                LogBook.Write(_logContent + " Error has occurred while fetching pager address list from 'GenNotifyPagers' table", ex, "CooperAtkins.NotificationClient.NotificationComposer.PagerNotificationComposer");
            }
            finally
            {
                notifyPagerAddressList.Dispose();
            }
            return notifyPagerAddressList;
        }

        /// <summary>
        /// Get Pager Notification ID
        /// </summary>
        /// <param name="notifyProfileID"></param>
        /// <returns></returns>
        private int GetPagerNotificationID(int notifyProfileID)
        {
            NotificationProfile notificationProfile = null;
            int pagerNotificationID = 0;
            try
            {
                notificationProfile = new NotificationProfile();
                notificationProfile.NotifyProfileID = notifyProfileID;
                notificationProfile = notificationProfile.Execute();
                pagerNotificationID = notificationProfile.PagerNotifyID;
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notificationProfile, this);

                /*Write exception log*/
                LogBook.Write(_logContent + " Error has occurred while fetching PagerNotifyID ", ex, "CooperAtkins.NotificationClient.NotificationComposer.PagerNotificationComposer");
            }
            finally
            {
                notificationProfile.Dispose();
            }
            return pagerNotificationID;
        }

        /// <summary>
        /// Get Email notification list
        /// </summary>
        /// <param name="notifyEmailAddressList"></param>
        /// <returns></returns>
        private List<INotifyObject> GetNotificationList(NotifyPagerAddressList notifyPagerAddressList, AlarmObject alarmObject)
        {
            List<INotifyObject> notifyList = new List<INotifyObject>();
            NotificationStyle notificationStyle = new NotificationStyle();
            foreach (NotifyPagerAddress notifyPagerAddress in notifyPagerAddressList)
            {
                /*Check for Notification Pager Address and exit the sending process, if NotificationPagerAddress object is NULL*/
                if (notifyPagerAddress != null)
                {
                    /*Get Notification Object*/
                    //if (notifyPagerAddress.DeliveryMethod == 1) // DeliveryMethod 0 = Modem 1 = SNPP
                    //{
                    /*Get SNPP notify settings*/
                    if (GetNotifySNPPObject(alarmObject, notifyPagerAddress) != null)
                        notifyList.Add(GetNotifySNPPObject(alarmObject, notifyPagerAddress));
                    //}
                }
                else
                {
                    /*Record notification if Email Address parameters are not supplied properly*/
                    notificationStyle.RecordNotification(ErrorMessages.PagerComposer_SNPPParmsNotSupplied, alarmObject.NotificationID, 0,NotifyStatus.FAIL, NotifyTypes.PAGER);
                }
            }
            return notifyList;
        }

        /// <summary>
        /// Get Notification settings for SNPP 
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <param name="notifyPagerAddress"></param>
        /// <returns></returns>
        private NotifyObject GetNotifySNPPObject(Interface.Alarm.AlarmObject alarmObject, NotifyPagerAddress notifyPagerAddress)
        {
            string snppServer = "", defaultPagerSubject = "", defaultPagerBody = "", defaultSubject = "", defaultBody = "";
            string subject = "", body = "", toAddress = "";
            int snppPort = 0;
            int pagerDelay = 0;
            string pagerMessage = string.Empty;

            /*Create Notification Style object*/
            NotificationStyle notificationStyle = new NotificationStyle();

            /*Create notification object */
            NotifyObject notifyObject = new NotifyObject();

            /*Set Notification Type*/
            notifyObject.NotificationType = "Pager";

            /*Create hashtable for notification settings*/
            Hashtable notificationSettings = new Hashtable();

            /*Set Default pager subject*/
            defaultPagerSubject = notificationStyle.GetFormatString(alarmObject, 1, "EmailSubjectPager");

            /*Set default pager body*/
            defaultPagerBody = notificationStyle.GetFormatString(alarmObject, 1, "EmailBodyPager");

            /*Set default subject*/
            defaultSubject = notificationStyle.GetFormatString(alarmObject, 1, "EmailSubject");

            /*Set default body*/
            defaultBody = notificationStyle.GetFormatString(alarmObject, 1, "EmailBody");

            /*If default pager subject is empty set default subject as default pager subject*/
            defaultPagerSubject = (defaultPagerSubject == string.Empty) ? defaultSubject : defaultPagerSubject;

            /*If default pager body is empty set default body as default pager body*/
            defaultPagerBody = (defaultPagerBody == string.Empty) ? defaultBody : defaultPagerBody;

            /*Set Email Body and Subject format*/
            subject = defaultPagerSubject;
            body = defaultPagerBody;

            subject = notificationStyle.SubstituteFormatString(subject, alarmObject);
            body = notificationStyle.SubstituteFormatString(body, alarmObject);

            /*Pager Body as notification data*/
            /*If pager body is empty assign the default pager body from alarm object*/

            /*Check this logic in old application*/
            /*In case of missed communication assign the custom body and subject*/
            if (alarmObject.IsMissCommNotification || body == string.Empty)
                body = alarmObject.PagerMessage;



            notificationSettings.Add("DeliveryMethod", notifyPagerAddress.DeliveryMethod);


            /*Get SNPP settings from GenStores*/
            GenStoreInfo genStoreInfo = null;

            /*Get GenStore Information*/
            genStoreInfo = GenStoreInfo.GetInstance();

            snppServer = genStoreInfo.SNPPServer;
            snppPort = genStoreInfo.SNPPPort;

            /*Set PhoneNumber to Pager destination field*/
            toAddress = notifyPagerAddress.PhoneNumber;




            /*Pager Subject*/
            notificationSettings.Add("PagerSubject", subject);


            notificationSettings.Add("Name", notifyPagerAddress.PagerName);


            /*Pager Destination Address*/
            notificationSettings.Add("ToAddress", toAddress);

            /* if the delivery type is modem*/
            if (notifyPagerAddress.DeliveryMethod != 1)
            {
                pagerDelay = notifyPagerAddress.PagerDelay;

                if (alarmObject.PagerMessage.Trim() != string.Empty)
                    pagerMessage = alarmObject.PagerMessage;
                else
                    pagerMessage = notifyPagerAddress.PagerMessage;

                pagerMessage = notificationStyle.SubstituteFormatString(pagerMessage, alarmObject);

                notificationSettings.Add("PagerDelay", pagerDelay);
                notificationSettings.Add("PagerMessage", pagerMessage);

                notificationSettings.Add("AttemptCount", "0");
                notificationSettings.Add("LastSentTime", DateTime.Now);
            }


            notificationSettings.Add("PagerComPort", genStoreInfo.PagerComPort);
            notificationSettings.Add("COMportInitString", genStoreInfo.ComPortInitStr);



            LogBook.Debug("SNPP Server: " + snppServer);
            LogBook.Debug("SNPP Port: " + snppPort);

            notificationSettings.Add("NotificationID", alarmObject.NotificationID);

            if (notifyPagerAddress.DeliveryMethod == 1)
            {
                if (snppServer != string.Empty)
                {
                    /*Set Default SNPP port if not assigned*/
                    snppPort = (snppPort < 1 || snppPort > 32767) ? 444 : snppPort;


                    /*If pager destination address is empty record notification as fail*/
                    if (toAddress != string.Empty)
                    {
                        /*SNPP Server or Host*/
                        notificationSettings.Add("SNPPServer", genStoreInfo.SNPPServer);

                        /*SNPP Port Number*/
                        notificationSettings.Add("SNPPPort", snppPort);

                    }
                    else
                    {
                        notificationStyle.RecordNotification(ErrorMessages.PagerComposer_SNPPToAddressNotSupplied, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.PAGER);

                        /*Exit and return empty notification object*/
                        return null;
                    }
                }
                else
                {
                    /*Record notification if SNPP settings are not supplied properly*/
                    notificationStyle.RecordNotification(ErrorMessages.PagerComposer_SNPPParmsNotSupplied, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.PAGER);

                    /*Exit and return empty notification object*/
                    return null;
                }

            }

            /*Set notification setting to notification object*/
            notifyObject.NotifierSettings = notificationSettings;

            /*Pager body*/
            notifyObject.NotificationData = body;

            return notifyObject;
        }

    }
}
