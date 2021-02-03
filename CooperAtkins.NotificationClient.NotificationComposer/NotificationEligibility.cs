/*
 *  File Name : NotificationClient.cs
 *  Author : Srinivas Rao Eranti 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/30/2010
 *  
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Collections.Generic;
    using EnterpriseModel.Net;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.NotificationClient.NotificationComposer.DataAccess;
    using CooperAtkins.Generic;


    public class NotificationEligibility
    {
        private int _notifyProfileID;

        private bool _isDynamicNotificationRemoved;
        private bool _isDynamicNotificationCleared;
        private bool _isProcessCompleted;
        private bool _isResumedNotification;


        public int IVRUserID { get; set; }
        public int NotificationSentCount { get; set; }

        public bool HasDynamicTypes { get; private set; }
        public bool HasPreviousMessageBoard { get; private set; }
        public bool HasPreviousSwitch { get; private set; }


        public NotificationEligibility(int notifyProfileID, bool isDynamicNotificationCleared, bool isDynamicNotificationRemoved, bool isProcessCompleted, bool isResumedNotification)
        {
            _notifyProfileID = notifyProfileID;
            _isDynamicNotificationRemoved = isDynamicNotificationRemoved;
            _isDynamicNotificationCleared = isDynamicNotificationCleared;
            _isProcessCompleted = isProcessCompleted;
            _isResumedNotification = isResumedNotification;
        }

        public string[] GetNotifyTypes()
        {
            List<string> notifyTypes = new List<string>();
            GenStoreInfo genStoreInfo = null;

            /*Get GenStore Information*/
            genStoreInfo = GenStoreInfo.GetInstance();

            /* 10/22/15 JH 
               below 3 lines of code are junk - this will never return NULL  */
            NotificationProfile notificationProfile = GetActiveNotificationProfile(genStoreInfo.StoreID);


            /*Check for IsAlertEnabled flag.If the flag is not set return null to stop all notifications*/
            if (notificationProfile.IsAlertEnabled)
                return null;

            /*get all notification type from configuration file.*/
            List<NotificationTypes> notificationTypelist = new NotificationTypes().GetNotificationTypes();

            IVRUserID = notificationProfile.IVR_UserID;

            HasDynamicTypes = false;

            /* Check for the alarm cleared or Check for the sensor comes to the normal range or the notification type is resumed type
             * then load only dynamic notification types */

            if ((_isProcessCompleted && NotificationSentCount > 0) || _isResumedNotification)
            {
                /*Stop notifications for email,popup,script,pager.... and check whether MessageBoard and Relay Switch are active or not */
                foreach (NotificationTypes notificationType in notificationTypelist)
                {
                    if ((notificationProfile.NotifyType & notificationType.Value).ToBoolean() && notificationType.IsDynamicType)
                    {
                        HasDynamicTypes = true;
                        notifyTypes.Add(notificationType.Name);
                        if (notificationType.Name == "MessageBoard")
                            HasPreviousMessageBoard = true;
                        if (notificationType.Name == "Switch")
                            HasPreviousSwitch = true;
                    }
                }
            }
            else
            {
                /* Otherwise load all configured notification types*/
                foreach (NotificationTypes notificationType in notificationTypelist)
                {
                    if ((notificationProfile.NotifyType & notificationType.Value).ToBoolean())
                    {

                        /*if this is to clear dynamic notification then do not add other types to  the notify types*/
                        if (_isDynamicNotificationCleared == true && _isDynamicNotificationRemoved == false)
                        {
                            if (notificationType.IsDynamicType)
                            {
                                notifyTypes.Add(notificationType.Name);
                                if (notificationType.Name == "MessageBoard")
                                    HasPreviousMessageBoard = true;
                                if (notificationType.Name == "Switch")
                                    HasPreviousSwitch = true;

                            }
                            continue;
                        }
                        /* checking whether the notification type is dynamic*/
                        if (notificationType.IsDynamicType)
                        {
                            HasDynamicTypes = true;
                            if (notificationType.Name == "MessageBoard")
                                HasPreviousMessageBoard = true;
                            if (notificationType.Name == "Switch")
                                HasPreviousSwitch = true;
                        }

                        /* don't add dynamic types if the user already cleared dynamic in previous escalation*/
                        if (_isDynamicNotificationRemoved && notificationType.IsDynamicType)
                        {
                            continue;
                        }

                        notifyTypes.Add(notificationType.Name);
                    }
                }
            }

            return notifyTypes.ToArray();
        }

        /// <summary>
        /// Get active notification profile ids 
        /// </summary>
        /// <returns></returns>
        private NotificationProfile GetActiveNotificationProfile(int storeID)
        {
            NotificationProfile notificationProfile = null;
            try
            {
                notificationProfile = new NotificationProfile();
                notificationProfile.NotifyProfileID = _notifyProfileID;
                notificationProfile.StoreID = storeID;
                notificationProfile = notificationProfile.Execute();

            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notificationProfile, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while fetching notification profile Ids ", ex, "CooperAtkins.NotificationClient.NotificationComposer.NotificationEligibility");
            }
            finally
            {
                notificationProfile.Dispose();
            }

            return notificationProfile;
        }

        /// <summary>
        /// Get E-mail Notification ID
        /// </summary>
        /// <param name="notifyProfileID"></param>
        /// <returns></returns>
        private int GetEmailNotificationID(int notifyProfileID)
        {
            NotificationProfile notificationProfile = null;
            int emailNotificationID = 0;
            try
            {
                /*Create NotificationProfile instance*/
                notificationProfile = new NotificationProfile();
                /*Send criteria notifyProfileID to get the Notification ID*/
                notificationProfile.NotifyProfileID = notifyProfileID;
                /*Execute*/
                notificationProfile = notificationProfile.Execute();
                /*Assign the EmailNotifyID to local variable emailNotificationID*/
                emailNotificationID = notificationProfile.EmailNotifyID;
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notificationProfile, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while fetching EmailNotifyID ", ex, "CooperAtkins.NotificationClient.NotificationComposer.EmailNotificationComposer");
            }
            finally
            {
                /*Dispose the notificationProfile object*/
                notificationProfile.Dispose();
            }
            return emailNotificationID;
        }

        /// <summary>
        /// Get Notification E-Mail Address list based on Notification Profile ID
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private NotifyEmailAddressList GetNotifyEmailAddressList(int notifyProfileID)
        {
            NotifyEmailAddressList notifyEmailAddressList = null;
            try
            {
                /*Create NotifyEmailAddressList object*/
                notifyEmailAddressList = new NotifyEmailAddressList();
                /*Create Criteria object*/
                Criteria criteria = new Criteria();
                /*Get the Email notificationId and assign to criteria.ID*/
                criteria.ID = GetEmailNotificationID(notifyProfileID);
                /*Gets list of notification email addresses*/
                notifyEmailAddressList.Load(criteria);
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notifyEmailAddressList, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while fetching email address list from 'GenNotifyEmails' table", ex, "CooperAtkins.NotificationClient.NotificationComposer.EmailNotificationComposer");
            }
            finally
            {
                notifyEmailAddressList.Dispose();
            }
            return notifyEmailAddressList;
        }


    }
}
